requireAuth();

const token   = () => localStorage.getItem('gp_token');
let connection = null;
let activeMatchId = null;
let myUserId = null;

function showToast(msg, type = '') {
    const t = document.getElementById('toast');
    t.textContent = msg; t.className = 'toast show ' + type;
    setTimeout(() => t.className = 'toast', 3000);
}

// Format time helper
function formatTime(isoString) {
    const d = new Date(isoString);
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

// Decode userId from JWT token (it's in the payload)
function getUserIdFromToken() {
    try {
        const payload = JSON.parse(atob(token().split('.')[1]));
        const key = Object.keys(payload).find(k => k.includes('nameidentifier'));
        return parseInt(payload[key]);
    } catch { return null; }
}

// Load the matches list in the left panel
function loadConversations() {
    $.ajax({
        url: '/api/match/list',
        method: 'GET',
        headers: { Authorization: 'Bearer ' + token() },
        success: (matches) => {
            const container = document.getElementById('conv-items');
            if (!matches.length) {
                container.innerHTML = `<div style="padding:20px;text-align:center;color:var(--text-light);font-size:13px">No matches yet</div>`;
                return;
            }

            container.innerHTML = matches.map(m => {
                const photo = m.photoUrl && m.photoUrl !== ''
                    ? m.photoUrl
                    : `https://ui-avatars.com/api/?name=${encodeURIComponent(m.name)}&background=C84B9E&color=fff`;
                return `
                  <div class="conv-item" id="conv-${m.matchId}" onclick="openMatch(${m.matchId}, '${m.name}', '${photo}')">
                    <img class="conv-avatar" src="${photo}" alt="${m.name}"
                      onerror="this.src='https://ui-avatars.com/api/?name=${encodeURIComponent(m.name)}&background=C84B9E&color=fff'" />
                    <div class="conv-info">
                      <div class="conv-name">${m.name}</div>
                      <div class="conv-msg">📍 ${m.city}</div>
                    </div>
                    <div class="online-dot"></div>
                  </div>`;
            }).join('');

            // Auto-open if matchId is in URL
            const params = new URLSearchParams(window.location.search);
            const urlMatchId = params.get('matchId');
            if (urlMatchId) {
                const match = matches.find(m => m.matchId == urlMatchId);
                if (match) {
                    const photo = match.photoUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(match.name)}&background=C84B9E&color=fff`;
                    openMatch(match.matchId, match.name, photo);
                }
            }
        },
        error: () => showToast('Failed to load conversations.', 'error')
    });
}

// Open a specific match chat
function openMatch(matchId, name, photo) {
    // Highlight active conversation
    document.querySelectorAll('.conv-item').forEach(c => c.classList.remove('active'));
    const el = document.getElementById(`conv-${matchId}`);
    if (el) el.classList.add('active');

    activeMatchId = matchId;

    // Show chat header and input
    document.getElementById('chat-header').style.display = 'flex';
    document.getElementById('chat-input-bar').style.display = 'flex';
    document.getElementById('chat-avatar').src = photo;
    document.getElementById('chat-avatar').onerror = function() {
        this.src = `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=C84B9E&color=fff`;
    };
    document.getElementById('chat-name').textContent = name;

    // Leave previous SignalR group
    if (connection && connection.state === 'Connected' && activeMatchId) {
        connection.invoke('LeaveMatch', activeMatchId).catch(() => {});
    }

    // Load message history
    loadHistory(matchId);

    // Join new SignalR group
    if (connection && connection.state === 'Connected') {
        connection.invoke('JoinMatch', matchId).catch(console.error);
    }
}

// Load message history from REST API
function loadHistory(matchId) {
    const container = document.getElementById('chat-messages');
    container.innerHTML = '<div class="spinner"></div>';

    $.ajax({
        url: `/api/message/${matchId}`,
        method: 'GET',
        headers: { Authorization: 'Bearer ' + token() },
        success: (res) => {
            const messages = Array.isArray(res) ? res : (res.messages || []);
            renderMessages(messages);
        },
        error: () => { container.innerHTML = '<p style="text-align:center;color:var(--text-light)">Could not load messages.</p>'; }
    });
}

// Render the full message history
function renderMessages(messages) {
    const container = document.getElementById('chat-messages');
    if (!myUserId) myUserId = getUserIdFromToken();

    if (!messages.length) {
        container.innerHTML = `<div style="text-align:center;color:var(--text-light);font-size:13px;margin-top:40px">No messages yet. Say hello! 👋</div>`;
        return;
    }

    container.innerHTML = messages.map(m => {
        const isSent = m.senderId === myUserId;
        return `
          <div class="msg-row ${isSent ? 'sent' : 'received'}">
            <div class="msg-bubble ${isSent ? 'sent' : 'received'}">${escapeHtml(m.body)}</div>
            <div class="msg-time">${formatTime(m.sentAt)}</div>
          </div>`;
    }).join('');

    // Scroll to bottom
    container.scrollTop = container.scrollHeight;
}

// Append a single new message bubble
function appendMessage(msg) {
    if (!myUserId) myUserId = getUserIdFromToken();
    const container = document.getElementById('chat-messages');

    // Remove empty state if present
    const empty = container.querySelector('div[style*="text-align:center"]');
    if (empty) empty.remove();

    const isSent = msg.senderId === myUserId;
    const div = document.createElement('div');
    div.className = `msg-row ${isSent ? 'sent' : 'received'}`;
    div.innerHTML = `
      <div class="msg-bubble ${isSent ? 'sent' : 'received'}">${escapeHtml(msg.body)}</div>
      <div class="msg-time">${formatTime(msg.sentAt)}</div>`;
    container.appendChild(div);
    container.scrollTop = container.scrollHeight;
}

// Send message via SignalR hub
function sendMessage() {
    const input = document.getElementById('msg-input');
    const body  = input.value.trim();
    if (!body || !activeMatchId) return;

    if (connection && connection.state === 'Connected') {
        connection.invoke('SendMessage', activeMatchId, body)
            .catch(err => showToast('Failed to send message.', 'error'));
        input.value = '';
    } else {
        showToast('Connection lost. Please refresh.', 'error');
    }
}

// Enter key to send
document.addEventListener('keydown', e => {
    if (e.key === 'Enter' && document.activeElement.id === 'msg-input') sendMessage();
});

// XSS protection
function escapeHtml(str) {
    return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

// Build SignalR connection
function connectSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chatHub?access_token=${token()}`)
        .withAutomaticReconnect()
        .build();

    // Receive messages in real time
    connection.on('ReceiveMessage', (msg) => {
        if (msg.matchId === activeMatchId) {
            appendMessage(msg);
        }
    });

    // Handle errors from hub
    connection.on('Error', (msg) => showToast(msg, 'error'));

    connection.onreconnected(() => {
        showToast('Reconnected!', 'success');
        if (activeMatchId) connection.invoke('JoinMatch', activeMatchId).catch(console.error);
    });

    connection.start()
        .then(() => {
            console.log('SignalR connected');
            if (activeMatchId) connection.invoke('JoinMatch', activeMatchId).catch(console.error);
        })
        .catch(err => showToast('Could not connect to chat server.', 'error'));
}

// Init
myUserId = getUserIdFromToken();
loadConversations();
connectSignalR();