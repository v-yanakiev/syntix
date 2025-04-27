import browser from "webextension-polyfill";
import { v5 as uuidv5 } from "uuid";
import {
  computePosition,
  autoUpdate,
  flip,
  shift,
  offset,
  arrow,
} from "@floating-ui/dom";
import hljs from "highlight.js";

const URL_NAMESPACE = "6ba7b810-9dad-11d1-80b4-00c04fd430c8";

interface ApiResponse {
  error?: string;
  ok: boolean;
  data?: unknown;
  text?: string;
}

interface SseMessage {
  type: "sseMessage" | "sseError" | "sseStatus" | "ping";
  data?: ApiResponse;
  error?: string;
  status?: string;
}

type MessageType =
  | "apiRequest"
  | "sseConnect"
  | "sseDisconnect"
  | "ping"
  | "pong";

async function makeApiRequest(
  urlSubdirectory: string,
  options?: RequestInit & { type?: MessageType }
): Promise<ApiResponse> {
  try {
    const response = (await browser.runtime.sendMessage({
      type: options?.type,
      urlSubdirectory,
      options: {
        ...options,
        credentials: "include",
      },
    })) as ApiResponse;

    return response;
  } catch (error) {
    console.error("[Syntix] Error:", error);
    return {
      ok: false,
      data: null,
      error: error instanceof Error ? error.message : "Unknown error",
    };
  }
}

interface PopoverElements {
  popover: HTMLDivElement;
  closeButton: HTMLButtonElement;
  contentContainer: HTMLPreElement;
  arrowElement: HTMLDivElement;
}

function createPopoverElements(): PopoverElements {
  const popover = document.createElement("div");
  popover.classList.add("syntix-popover");

  const closeButton = document.createElement("button");
  closeButton.textContent = "×";
  closeButton.classList.add("syntix-close-button");

  const contentContainer = document.createElement("pre");
  contentContainer.classList.add("syntix-content-container");

  const arrowElement = document.createElement("div");
  arrowElement.classList.add("syntix-arrow");

  popover.appendChild(closeButton);
  popover.appendChild(contentContainer);
  popover.appendChild(arrowElement);

  return { popover, closeButton, contentContainer, arrowElement };
}

function setupPopoverPositioning(
  button: HTMLButtonElement,
  elements: PopoverElements,
  cleanup: (() => void) | null
): (() => void) | null {
  const updatePosition = () => {
    computePosition(button, elements.popover, {
      placement: "bottom",
      middleware: [
        offset(6),
        flip(),
        shift(),
        arrow({ element: elements.arrowElement }),
      ],
    }).then(({ x, y, placement, middlewareData }) => {
      Object.assign(elements.popover.style, {
        left: `${x}px`,
        top: `${y}px`,
      });

      const { x: arrowX, y: arrowY } = middlewareData.arrow!;
      const staticSide = {
        top: "bottom",
        right: "left",
        bottom: "top",
        left: "right",
      }[placement.split("-")[0]];

      Object.assign(elements.arrowElement.style, {
        left: arrowX != null ? `${arrowX}px` : "",
        top: arrowY != null ? `${arrowY}px` : "",
        [staticSide!]: "-4px",
      });
    });
  };

  updatePosition();
  return autoUpdate(button, elements.popover, updatePosition);
}

async function handleExecuteButtonClick(
  button: HTMLButtonElement,
  codeBlock: Element
): Promise<void> {
  try {
    const currentUrl = window.location.href + window.location.search;
    const correspondingChatId = uuidv5(currentUrl, URL_NAMESPACE);
    const bodyToSend = {
      chatId: correspondingChatId,
      code: codeBlock.textContent || "",
    };

    const elements = createPopoverElements();
    document.body.appendChild(elements.popover);

    let cleanup: (() => void) | null = null;
    let isFirstMessage = true;

    const handleClickOutside = (event: MouseEvent) => {
      if (
        !elements.popover.contains(event.target as Node) &&
        !button.contains(event.target as Node) &&
        elements.popover.style.display !== "none"
      ) {
        elements.popover.style.display = "none";
        cleanup?.();
        cleanup = null;
        document.removeEventListener("click", handleClickOutside);
      }
    };

    elements.closeButton.addEventListener("click", () => {
      elements.popover.style.display = "none";
      cleanup?.();
      cleanup = null;
    });

    document.addEventListener("click", handleClickOutside);

    const messageListener = (message: unknown) => {
      const sseMessage = message as SseMessage;

      if (!cleanup) {
        cleanup = setupPopoverPositioning(button, elements, cleanup);
      }

      if (sseMessage.type === "ping") {
        browser.runtime.sendMessage({ type: "pong" });
        return;
      }

      switch (sseMessage.type) {
        case "sseMessage":
          if (sseMessage.data?.text) {
            if (isFirstMessage) {
              elements.contentContainer.innerHTML = sseMessage.data.text;
              isFirstMessage = false;
            } else {
              elements.contentContainer.innerHTML += sseMessage.data.text;
            }
            elements.contentContainer.innerHTML = getContentWithHighlightedCode(
              elements.contentContainer.innerHTML
            );
          }
          break;

        case "sseError":
          if (sseMessage.error) {
            const errorMsg = sseMessage.error.includes("Details:")
              ? sseMessage.error
              : "Connection error. Please ensure you're logged in at https://app.syntix.pro";
            elements.contentContainer.textContent += errorMsg;
          }
          break;

        case "sseStatus":
          if (sseMessage.status) {
            elements.contentContainer.textContent += sseMessage.status;
          }
          break;

        default:
          console.warn("[Syntix] Unknown message type:", sseMessage);
      }
    };

    browser.runtime.onMessage.addListener(messageListener);

    elements.contentContainer.textContent = "Executing...";
    elements.popover.style.display = "block";
    cleanup = setupPopoverPositioning(button, elements, cleanup);

    const response = await makeApiRequest(`/execution/new`, {
      type: "sseConnect",
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(bodyToSend),
    });

    if (!response.ok) {
      throw new Error("Failed to establish connection");
    }

    const cleanupAll = async () => {
      document.removeEventListener("click", handleClickOutside);
      browser.runtime.onMessage.removeListener(messageListener);
      await makeApiRequest(`/execution/new`, {
        type: "sseDisconnect",
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(bodyToSend),
      });

      cleanup?.();
      cleanup = null;
      elements.popover.remove();
    };

    window.addEventListener("unload", cleanupAll);
    button.addEventListener("click", cleanupAll);
  } catch (error) {
    console.error("[Syntix] Error:", error);
    alert(
      error instanceof Error
        ? error.message
        : "Failed to execute code. Please ensure you're logged in at https://app.syntix.pro"
    );
  }
}

function addExecuteButton(codeBlock: Element): void {
  if (codeBlock.querySelector(".syntix-code-executing-button")) {
    return;
  }

  const parentElement = codeBlock.parentElement;
  if (parentElement?.querySelector(".syntix-code-executing-button")) {
    return;
  }

  const button = document.createElement("button");
  button.classList.add("syntix-code-executing-button");
  button.textContent = "Execute Code";

  button.addEventListener("click", () =>
    handleExecuteButtonClick(button, codeBlock)
  );

  const container = parentElement || codeBlock;
  const computedStyle = window.getComputedStyle(container);
  if (computedStyle.position === "static") {
    (container as HTMLElement).style.position = "relative";
  }

  container.appendChild(button);
}

const processedElements = new WeakSet<Element>();

function findAndAddButtonsToCodeBlocks(): void {
  const elements = document.querySelectorAll("pre code");
  elements.forEach((element) => {
    if (element.closest(".syntix-popover")) {
      return;
    }

    if (!processedElements.has(element)) {
      processedElements.add(element);
      addExecuteButton(element);
    }
  });
}

function getContentWithHighlightedCode(popoverContent: string): string {
  const codeBlockRegex = /```([\s\S]*?)```/g;
  return popoverContent.replace(codeBlockRegex, (match, codeContent) => {
    const preElement = document.createElement("pre");
    const codeElement = document.createElement("code");
    codeElement.textContent = codeContent.trim();
    hljs.highlightElement(codeElement);
    preElement.appendChild(codeElement);
    return preElement.outerHTML;
  });
}

let observer: MutationObserver | null = null;

function initCodeBlockObserver(): void {
  if (observer) {
    return;
  }

  findAndAddButtonsToCodeBlocks();

  observer = new MutationObserver((mutations) => {
    if (mutations.some((mutation) => mutation.addedNodes.length > 0)) {
      findAndAddButtonsToCodeBlocks();
    }
  });

  observer.observe(document.body, {
    childList: true,
    subtree: true,
  });
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initCodeBlockObserver);
} else {
  initCodeBlockObserver();
}

window.addEventListener("unload", () => {
  if (observer) {
    observer.disconnect();
    observer = null;
  }
});

// Create a separate CSS file for styles
const styles = `
  .syntix-code-executing-button {
    position: absolute;
    top: 5px;
    right: 5px;
    background-color: #000;
    color: #fff;
    padding: 5px 10px;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    z-index: 1000;
  }

  .syntix-popover {
    position: absolute;
    background: #333;
    color: white;
    padding: 10px;
    padding-top: 25px;
    border-radius: 6px;
    font-size: 14px;
    max-width: 40rem;
    max-height: 30rem;
    overflow-y: auto;
    z-index: 1001;
    display: none;
  }

  .syntix-close-button {
    position: absolute;
    top: 5px;
    right: 5px;
    background: none;
    border: none;
    color: white;
    font-size: 18px;
    cursor: pointer;
    padding: 0;
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0.7;
    transition: opacity 0.2s;
  }

  .syntix-content-container {
    margin: 0;
    padding-right: 15px;
    white-space: pre-wrap;
    word-break: break-word;
  }

  .syntix-arrow {
    position: absolute;
    width: 8px;
    height: 8px;
    background: #333;
    transform: rotate(45deg);
  }

  .hljs {
    display: block;
    overflow-x: auto;
    padding: 1em;
    background: #2d2d2d;
    color: #ccc;
  }

  .hljs-keyword,
  .hljs-selector-tag,
  .hljs-tag {
    color: #e78c45;
  }

  .hljs-string {
    color: #b9ca4a;
  }

  .hljs-number {
    color: #6c99bb;
  }

  .hljs-variable,
  .hljs-template-variable {
    color: #d54e53;
  }

  .hljs-comment {
    color: #7a7a7a;
  }
`;

const styleElement = document.createElement("style");
styleElement.textContent = styles;
document.head.appendChild(styleElement);
