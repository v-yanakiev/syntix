using System.Text.Json;
using aiExecBackend.Extensions;
using Microsoft.AspNetCore.Identity;
using Models;
using Stripe;
using Stripe.Checkout;
using Stripe.Forwarding;
using Stripe.V2;

namespace aiExecBackend.Endpoints;

public static class StripeWebhookEndpoints
{
    public static async Task<IResult> SessionCompletedHandler(HttpContext httpContext, PostgresContext postgresContext, IConfiguration configuration)
    {
        var request=httpContext.Request;
        var json = await new StreamReader(httpContext.Request.Body).ReadToEndAsync();
        var endpointSecret = configuration["StripeKey_SessionCompleted"] ?? throw new Exception("StripeKey_SessionCompleted not set!");
        try
        {
            _ = EventUtility.ParseEvent(json);
            var signatureHeader = request.Headers["Stripe-Signature"];

            var stripeEvent = EventUtility.ConstructEvent(json,
                signatureHeader, endpointSecret);

            switch (stripeEvent.Type)
            {
                // If on SDK version < 46, use class Events instead of EventTypes
                case EventTypes.PaymentIntentSucceeded:
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    
                    Console.WriteLine("A successful payment for {0} was made.", paymentIntent!.Amount);
                    // Then define and call a method to handle the successful payment intent.
                    // handlePaymentIntentSucceeded(paymentIntent);
                    break;
                }
                case EventTypes.PaymentMethodAttached:
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    // Then define and call a method to handle the successful attachment of a PaymentMethod.
                    // handlePaymentMethodAttached(paymentMethod);
                    break;
                }
                case EventTypes.CheckoutSessionCompleted:
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session == null)
                    {
                        var failureLogMessage =
                            $"session was null in CheckoutSessionCompleted for the event with Id='{stripeEvent.Id}'.";
                        throw new Exception(failureLogMessage);
                    }
                    
                    // Get the payment intent ID if you need to interact with the payment
                    var paymentIntentId = session.PaymentIntentId;
                    
                    if (session.PaymentStatus != "paid")
                    {
                        break;
                    }

                    var userId = session.ClientReferenceId;
                    var user = userId==null ? null: await postgresContext.Users.FindAsync(userId);
                    if (user == null)
                    {
                        var failureLogMessage = $"FAILURE 1: Could not connect payment of {session.AmountTotal} USD " +
                                                $"in session ID {session.Id}, subscription ID {session.SubscriptionId}," +
                                                $" and e-mail {session.Customer.Email} " +
                                                $"with non-existing user (userId = {userId})";
                        
                        postgresContext.Logs.Add(new Log() { Content =  failureLogMessage});
                        await postgresContext.SaveChangesAsync();
                        var service = new SubscriptionService();
                        await service.CancelAsync(session.SubscriptionId);
                        return Results.UnprocessableEntity();
                    }

                    user.Balance = 5;
                    user.SubscriptionId = session.SubscriptionId;
                    
                    var logMessage = $"SUCCESS 1: Connected payment of {session.AmountTotal} USD " +
                                  $"in session ID {session.Id}, subscription ID {session.SubscriptionId}," +
                                  $" and e-mail {session.Customer.Email} " +
                                  $"with existing user (userId = {userId})";
                    
                    
                    postgresContext.Logs.Add(new Log() { Content =  logMessage});

                    await postgresContext.SaveChangesAsync();
                    
                    break;
                }
            }
            return Results.Ok();
        }
        catch (StripeException e)
        {
            var exceptionLogMessage = $"FAILURE 2: StripeException in SessionCompletedHandler: {e.Message}";
            postgresContext.Logs.Add(new Log() { Content =exceptionLogMessage  });
            await postgresContext.SaveChangesAsync();

            return Results.BadRequest();
        }
        catch (Exception e)
        {
            var exceptionLogMessage = $"FAILURE 3: Exception in SessionCompletedHandler: {e.Message}";
            postgresContext.Logs.Add(new Log() { Content =exceptionLogMessage  });
            await postgresContext.SaveChangesAsync();
            
            return Results.InternalServerError();
        }
    }
}