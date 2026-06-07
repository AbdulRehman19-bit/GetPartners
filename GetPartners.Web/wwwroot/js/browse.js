requireAuth();

let profiles = [];
let currentIndex = 0;
let currentMatchId = null;

const token = () => localStorage.getItem('gp_token');

function showToast(msg, type = '') {
    const t = document.getElementById('toast');
    t.textContent = msg; t.className = 'toast show ' + type;
    setTimeout(() => t.className = 'toast', 3000);
}

// Load all browse profiles from the API
function loadProfiles() {
    $.ajax({
        url: '/api/browse',
        method: 'GET',
        headers: { Authorization: 'Bearer ' + token() },
        success: (res) => {
            profiles = Array.isArray(res) ? res : (res.profiles || []);
            currentIndex = 0;
            renderCard();
        },
        error: () => showToast('Failed to load profiles.', 'error')
    });
}

// Render the current profile card
function renderCard() {
    const container = document.getElementById('card-container');
    const buttons   = document.getElementById('action-buttons');
    const chips     = document.getElementById('interest-chips');

    if (currentIndex >= profiles.length) {
        container.innerHTML = `
          <div class="empty-state">
            <svg viewBox="0 0 24 24"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <p>No more profiles right now.<br>Check back later!</p>
          </div>`;
        buttons.style.display = 'none';
        chips.innerHTML = '';
        return;
    }

    const p = profiles[currentIndex];
    const photoUrl = p.photoUrl && p.photoUrl !== ''
        ? p.photoUrl
        : `https://ui-avatars.com/api/?name=${encodeURIComponent(p.name)}&background=C84B9E&color=fff&size=300`;

    container.innerHTML = `
      <div class="profile-card">
        <img src="${photoUrl}" alt="${p.name}" onerror="this.src='https://ui-avatars.com/api/?name=${encodeURIComponent(p.name)}&background=C84B9E&color=fff&size=300'" />
        <div class="card-overlay"></div>
        <div class="card-info">
          <div class="card-name">${p.name}, ${p.age}</div>
          <div class="card-city">📍 ${p.city}</div>
        </div>
      </div>`;

    buttons.style.display = 'flex';

    // Render interest chips if bio has keywords (placeholder logic)
    chips.innerHTML = ['Marriage', 'Family', 'Travel']
        .map(i => `<span class="chip">✦ ${i}</span>`).join('');
}

// Like the current profile
function likeProfile() {
    if (currentIndex >= profiles.length) return;
    const target = profiles[currentIndex];

    $.ajax({
        url: '/api/match/like',
        method: 'POST',
        contentType: 'application/json',
        headers: { Authorization: 'Bearer ' + token() },
        data: JSON.stringify({ targetUserId: target.userId }),
        success: (res) => {
            if (res.isMatch) {
                // Show match celebration overlay
                document.getElementById('match-photo').src =
                    target.photoUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(target.name)}&background=C84B9E&color=fff`;
                document.getElementById('match-name-text').textContent =
                    `You and ${target.name} liked each other!`;
                currentMatchId = res.matchId;
                document.getElementById('match-overlay').classList.add('show');
            } else {
                showToast('Like sent! ❤️', 'success');
                currentIndex++;
                renderCard();
            }
        },
        error: () => showToast('Something went wrong.', 'error')
    });
}

// Pass the current profile
function passProfile() {
    if (currentIndex >= profiles.length) return;
    const target = profiles[currentIndex];

    $.ajax({
        url: '/api/match/pass',
        method: 'POST',
        contentType: 'application/json',
        headers: { Authorization: 'Bearer ' + token() },
        data: JSON.stringify({ targetUserId: target.userId }),
        success: () => { currentIndex++; renderCard(); },
        error: () => { currentIndex++; renderCard(); }
    });
}

function loadNextProfile() { currentIndex++; renderCard(); }

function closeMatch() {
    document.getElementById('match-overlay').classList.remove('show');
    currentIndex++;
    renderCard();
}

function openChat() {
    if (currentMatchId) {
        window.location.href = `/pages/chat.html?matchId=${currentMatchId}`;
    }
    closeMatch();
}

function togglePref(el) {
    document.querySelectorAll('.pref-chip').forEach(c => c.classList.remove('active'));
    el.classList.add('active');
}

// Init
loadProfiles();