const authButton = document.getElementById("authButton");
const sendButton = document.getElementById("sendButton");
const messageInput = document.getElementById("message");
const output = document.getElementById("output");
const status = document.getElementById("status");

const setStatus = (text, kind = "default") => {
  status.className = "status";
  if (kind === "success") {
    status.classList.add("status--success");
  }
  if (kind === "error") {
    status.classList.add("status--error");
  }

  status.textContent = text;
};

const setBusy = (busy, actionText) => {
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

    const data = await response.json();
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

    const data = await response.json();

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
