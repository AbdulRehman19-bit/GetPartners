requireAuth();

const token = () => localStorage.getItem('gp_token');

function loadMatches() {
    $.ajax({
        url: '/api/match/list',
        method: 'GET',
        headers: { Authorization: 'Bearer ' + token() },
        success: (matches) => {
            const grid = document.getElementById('matches-grid');
            document.getElementById('match-count').textContent =
                `${matches.length} mutual match${matches.length !== 1 ? 'es' : ''}`;

            if (!matches.length) {
                grid.innerHTML = `<div class="empty-state" style="grid-column:1/-1">
                  <svg viewBox="0 0 24 24"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"/></svg>
                  <p>No matches yet. Keep browsing!</p>
                </div>`;
                return;
            }

            grid.innerHTML = matches.map(m => {
                const photo = m.photoUrl && m.photoUrl !== ''
                    ? m.photoUrl
                    : `https://ui-avatars.com/api/?name=${encodeURIComponent(m.name)}&background=C84B9E&color=fff&size=300`;
                return `
                  <a class="match-card" href="/pages/chat.html?matchId=${m.matchId}">
                    <img src="${photo}" alt="${m.name}"
                      onerror="this.src='https://ui-avatars.com/api/?name=${encodeURIComponent(m.name)}&background=C84B9E&color=fff&size=300'" />
                    <div class="match-card-overlay"></div>
                    <div class="match-badge">Match ✓</div>
                    <div class="match-info">
                      <div class="match-name">${m.name}</div>
                      <div class="match-sub">📍 ${m.city}</div>
                    </div>
                  </a>`;
            }).join('');
        },
        error: () => {
            document.getElementById('matches-grid').innerHTML =
                '<p style="color:var(--text-secondary)">Failed to load matches.</p>';
        }
    });
}

loadMatches();