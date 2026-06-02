type StatusKind = "default" | "success" | "error";

type ChatResponse = {
  message?: string;
};

type AuthStartResponse = {
  authorizationUrl: string;
};

const authButton = document.getElementById("authButton") as HTMLButtonElement;
const sendButton = document.getElementById("sendButton") as HTMLButtonElement;
const messageInput = document.getElementById("message") as HTMLTextAreaElement;
const output = document.getElementById("output") as HTMLPreElement;
const status = document.getElementById("status") as HTMLDivElement;

const setStatus = (text: string, kind: StatusKind = "default") => {
  status.className = "status";
  if (kind === "success") {
    status.classList.add("status--success");
  }
  if (kind === "error") {
    status.classList.add("status--error");
  }

  status.textContent = text;
};

const setBusy = (busy: boolean, actionText?: string) => {
  authButton.disabled = busy;
  sendButton.disabled = busy;

  if (actionText) {
    output.textContent = actionText;
  }
};

authButton.addEventListener("click", async () => {
  setBusy(true, "Preparing Salesforce authorization...");
  setStatus("Starting OAuth flow...");

  try {
    const response = await fetch("/api/auth/salesforce/start");
    if (!response.ok) {
      throw new Error(await response.text());
    }

    const data = (await response.json()) as AuthStartResponse;
    setStatus("Redirecting to Salesforce...", "success");
    window.location.href = data.authorizationUrl;
  } catch (error) {
    const message = error instanceof Error ? error.message : "Unable to start Salesforce login.";
    setStatus("Authentication failed", "error");
    output.textContent = message;
    setBusy(false);
  }
});

const sendMessage = async () => {
  const message = messageInput.value.trim();
  if (!message) {
    setStatus("Message is required", "error");
    output.textContent = "Please enter a prompt before sending.";
    messageInput.focus();
    return;
  }

  setBusy(true, "Sending message to agent...");
  setStatus("Awaiting response...");

  try {
    const response = await fetch("/api/chat", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ message }),
    });

    const data = (await response.json()) as ChatResponse;

    if (!response.ok) {
      const errorMessage = data.message ?? JSON.stringify(data, null, 2);
      throw new Error(errorMessage);
    }

    output.textContent = data.message ?? JSON.stringify(data, null, 2);
    setStatus("Response received", "success");
  } catch (error) {
    const messageText = error instanceof Error ? error.message : "Request failed.";
    output.textContent = messageText;
    setStatus("Request failed", "error");
  } finally {
    setBusy(false);
  }
};

sendButton.addEventListener("click", sendMessage);
messageInput.addEventListener("keydown", (event) => {
  const wantsSend = event.key === "Enter" && (event.ctrlKey || event.metaKey);
  if (wantsSend) {
    event.preventDefault();
    void sendMessage();
  }
});

setStatus("Not authenticated");
