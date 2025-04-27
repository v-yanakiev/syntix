import browser from "webextension-polyfill";

// Configuration
const config = {
  apiBaseUrl: "https://app.syntix.pro/api",
  // apiBaseUrl: import.meta.env.VITE_API_BASE_URL || "http://localhost:8080",
  pingInterval: 5000,
};

// Types
type MessageType = "sseConnect" | "sseDisconnect" | "ping" | "pong";

interface ApiRequest {
  type: MessageType;
  urlSubdirectory: string;
  options?: RequestInit;
}

interface ApiResponse {
  error?: string;
  ok: boolean;
  data: unknown;
}

interface SSEConnection {
  close: () => void;
}

// State management
const sseConnections = new Map<string, SSEConnection>();

// Type guards
const isApiRequest = (msg: unknown): msg is ApiRequest => {
  return (
    typeof msg === "object" &&
    msg !== null &&
    "type" in msg &&
    ["apiRequest", "sseConnect", "sseDisconnect", "pong"].includes(
      (msg as ApiRequest).type
    ) &&
    "urlSubdirectory" in msg &&
    typeof (msg as ApiRequest).urlSubdirectory === "string"
  );
};

// SSE handling
const processSSEData = async (
  reader: ReadableStreamDefaultReader<Uint8Array>,
  decoder: TextDecoder,
  tabId: number
): Promise<void> => {
  try {
    while (true) {
      const { value, done } = await reader.read();
      if (done) break;

      const text = decoder.decode(value, { stream: true });
      const lines = text.split("\n");

      for (const line of lines) {
        if (!line.startsWith("data: ")) continue;

        try {
          const data = line.slice(6).trim();
          if (!data) continue;

          const parsedData = JSON.parse(data);
          await browser.tabs.sendMessage(tabId, {
            type: "sseMessage",
            data: parsedData,
          });
        } catch (error) {
          await browser.tabs.sendMessage(tabId, {
            type: "sseError",
            error: "Invalid JSON in SSE message",
          });
        }
      }
    }
  } catch (error) {
    await browser.tabs.sendMessage(tabId, {
      type: "sseError",
      error: "Stream processing error",
    });
  }
};

// Connection management
const cleanup = (
  reader: ReadableStreamDefaultReader<Uint8Array>,
  pingInterval: number,
  tabId: string
): void => {
  reader.cancel();
  clearInterval(pingInterval);
  sseConnections.delete(tabId);
};

const sendPing = async (
  tabId: number,
  reader: ReadableStreamDefaultReader<Uint8Array>,
  pingInterval: number
): Promise<void> => {
  try {
    await browser.tabs.sendMessage(tabId, { type: "ping" });
  } catch (error) {
    cleanup(reader, pingInterval, tabId.toString());
  }
};

// Message handlers
const handlePongMessage = (
  sendResponse: (response: ApiResponse) => void
): void => {
  sendResponse({ ok: true, data: "pong received" });
};

const handleSSEConnect = async (
  message: ApiRequest,
  sender: browser.Runtime.MessageSender,
  sendResponse: (response: ApiResponse) => void
): Promise<void> => {
  const tabId = sender.tab?.id;
  if (!tabId) {
    sendResponse({ ok: false, data: null, error: "Invalid tab ID" });
    return;
  }

  const existingConnection = sseConnections.get(tabId.toString());
  if (existingConnection) {
    existingConnection.close();
  }

  try {
    const response = await fetch(
      `${config.apiBaseUrl}${message.urlSubdirectory}`,
      {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: message.options?.body,
      }
    );

    if (!response.ok) {
      throw new Error("Failed to submit code execution request");
    }

    const reader = response.body?.getReader();
    if (!reader) {
      throw new Error("Failed to get response reader");
    }

    const decoder = new TextDecoder();
    const pingInterval = setInterval(
      () => sendPing(tabId, reader, pingInterval),
      config.pingInterval
    );

    processSSEData(reader, decoder, tabId).catch((error) => {
      cleanup(reader, pingInterval, tabId.toString());
    });

    sseConnections.set(tabId.toString(), {
      close: () => cleanup(reader, pingInterval, tabId.toString()),
    });

    sendResponse({ ok: true, data: "SSE connection established" });
  } catch (error) {
    sendResponse({
      ok: false,
      data: null,
      error: error instanceof Error ? error.message : "Unknown error",
    });
  }
};

const handleSSEDisconnect = (
  sender: browser.Runtime.MessageSender,
  sendResponse: (response: ApiResponse) => void
): void => {
  const connection = sseConnections.get(sender.tab?.id?.toString() || "");
  if (connection) {
    connection.close();
  }
  sendResponse({ ok: true, data: "SSE connection closed" });
};

// Message listener
browser.runtime.onMessage.addListener(
  (
    message: unknown,
    sender: browser.Runtime.MessageSender,
    sendResponse: (response: ApiResponse) => void
  ): true => {
    if (!isApiRequest(message)) {
      sendResponse({
        ok: false,
        data: null,
        error: "Invalid message type",
      });
      return true;
    }

    switch (message.type) {
      case "pong":
        handlePongMessage(sendResponse);
        break;
      case "sseConnect":
        void handleSSEConnect(message, sender, sendResponse);
        break;
      case "sseDisconnect":
        handleSSEDisconnect(sender, sendResponse);
        break;
    }

    return true;
  }
);

// Extension lifecycle
browser.runtime.onInstalled.addListener(() => {
  // Initialize any required extension state here
});
