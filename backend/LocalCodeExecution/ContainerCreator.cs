using Docker.DotNet;
using Docker.DotNet.Models;

namespace LocalExecution;

public static class ContainerCreator
{
    public static Task<CreateContainerResponse> Create(string imageName, DockerClient dockerClient)
    {
        var containerCreationResponse = dockerClient.Containers.CreateContainerAsync(
            new CreateContainerParameters
            {
                Name = "aiexec",
                Image = "vasil2000yanakiev/aiexec:latest",
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            "65432/tcp",
                            new List<PortBinding> { new() { HostPort = "65432" } }
                        }
                    }
                }
            });
        return containerCreationResponse;
    }
}