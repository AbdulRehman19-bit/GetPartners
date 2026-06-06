const API = '';  // empty = same origin as the ASP.NET app

// Save token and redirect
function saveTokenAndRedirect(token) {
    localStorage.setItem('gp_token', token);
    window.location.href = '/pages/setup-profile.html';
}

// Show toast message
function showToast(msg, type = '') {
    const t = document.getElementById('toast');
    t.textContent = msg;
    t.className = 'toast show ' + type;
    setTimeout(() => t.className = 'toast', 3000);
}

// Register
function register() {
    const email    = document.getElementById('reg-email').value.trim();
    const password = document.getElementById('reg-password').value;

    if (!email || !password) {
        showToast('Please fill in all fields.', 'error'); return;
    }
    if (password.length < 6) {
        showToast('Password must be at least 6 characters.', 'error'); return;
    }

    $.ajax({
        url: API + '/api/auth/register',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email, password }),
        success: (res) => {
            showToast('Account created!', 'success');
            saveTokenAndRedirect(res.token);
        },
        error: (xhr) => {
            const msg = xhr.responseJSON?.message || 'Registration failed.';
            showToast(msg, 'error');
        }
    });
}

// Login
function login() {
    const email    = document.getElementById('login-email').value.trim();
    const password = document.getElementById('login-password').value;

    if (!email || !password) {
        showToast('Please fill in all fields.', 'error'); return;
    }

    $.ajax({
        url: API + '/api/auth/login',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email, password }),
        success: (res) => {
            showToast('Welcome back!', 'success');
            // If profile exists go to browse, else setup
            localStorage.setItem('gp_token', res.token);
            checkProfileAndRedirect();
        },
        error: (xhr) => {
            const msg = xhr.responseJSON?.message || 'Invalid credentials.';
            showToast(msg, 'error');
        }
    });
}

function checkProfileAndRedirect() {
    $.ajax({
        url: API + '/api/profile',
        method: 'GET',
        headers: { Authorization: 'Bearer ' + localStorage.getItem('gp_token') },
        success: () => { window.location.href = '/pages/browse.html'; },
        error: () => { window.location.href = '/pages/setup-profile.html'; }
    });
}

// Guard - redirect to login if no token
function requireAuth() {
    if (!localStorage.getItem('gp_token')) {
        window.location.href = '/pages/index.html';
    }
}

// Logout
function logout() {
    localStorage.removeItem('gp_token');
    window.location.href = '/pages/index.html';
}