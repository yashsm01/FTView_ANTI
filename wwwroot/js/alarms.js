const POLL_INTERVAL = 5000;
const STORAGE_KEY = 'unclearedAlarms';

let lastCheckTime = null;
let unclearedAlarms = new Map();
let allAlarms = [];
let filteredAlarms = [];
let seenAlarmIds = new Set();

let currentPage = 1;
let currentPageSize = 10;
let totalPages = 1;

function loadUnclearedAlarms() {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
        try {
            const alarms = JSON.parse(stored);
            alarms.forEach(alarm => unclearedAlarms.set(alarm.eventID, alarm));
            updateAlertPanel();
        } catch (e) { console.error('Storage error:', e); }
    }
}

function saveUnclearedAlarms() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(Array.from(unclearedAlarms.values())));
}

function updateAlertPanel() {
    const panelContent = document.getElementById('panelContent');
    const badge = document.getElementById('alertBadge');
    const count = unclearedAlarms.size;

    badge.textContent = count;

    // Toggle whole-page blink and audio if alerts exist
    if (count > 0) {
        document.body.classList.add('alert-active-blink');
        if (window.alarmSound) window.alarmSound.start();
    } else {
        document.body.classList.remove('alert-active-blink');
        if (window.alarmSound) window.alarmSound.stop();
    }

    if (count === 0) {
        panelContent.innerHTML = '<div style="text-align: center; color: var(--secondary); padding: 2rem;">No active alerts trace.</div>';
        return;
    }

    panelContent.innerHTML = '';
    unclearedAlarms.forEach((alarm, eventID) => {
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert-card severity-${alarm.severity || 2}`;

        const date = alarm.eventTimeStamp ? new Date(alarm.eventTimeStamp).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' }) : 'N/A';

        alertDiv.innerHTML = `
            <div style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 0.75rem;">
                <span style="font-size: 0.75rem; font-weight: 700; color: var(--text-muted)">OCCURRED: ${date}</span>
                <button class="btn btn-outline" style="padding: 0.25rem; border-radius: 4px" onclick="clearAlert('${eventID}')">
                    <i data-lucide="x" style="width: 14px; height: 14px"></i>
                </button>
            </div>
            <div style="font-weight: 600; font-size: 0.9375rem; margin-bottom: 0.5rem; color: var(--primary)">${alarm.message || 'System Notification'}</div>
            <div style="font-size: 0.8125rem; color: var(--text-muted)">
                <strong>Source:</strong> ${alarm.sourceName || 'General'}<br>
                <strong>Category:</strong> ${alarm.eventCategory || 'Uncategorized'}
            </div>
        `;
        panelContent.appendChild(alertDiv);
    });
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

function addNewAlert(alarm) {
    if (!unclearedAlarms.has(alarm.eventID)) {
        unclearedAlarms.set(alarm.eventID, alarm);
        updateAlertPanel();
        saveUnclearedAlarms();
        if (alarm.sourceName) syncTagFromAlarm(alarm.sourceName);
    }
}

function clearAlert(eventID) {
    unclearedAlarms.delete(eventID);
    updateAlertPanel();
    saveUnclearedAlarms();
}

function clearAllAlerts() {
    if (unclearedAlarms.size === 0) return;
    if (confirm('Acknowledge and clear all active alerts?')) {
        unclearedAlarms.clear();
        updateAlertPanel();
        saveUnclearedAlarms();
    }
}

function toggleAlertPanel() {
    document.getElementById('alertPanel').classList.toggle('open');
    document.getElementById('overlay').classList.toggle('active');
}

function closeAlertPanel() {
    document.getElementById('alertPanel').classList.remove('open');
    document.getElementById('overlay').classList.remove('active');
}

async function syncTagFromAlarm(sourceName) {
    try {
        const apiBase = window.location.origin;
        await fetch(`${apiBase}/api/tags/sync-from-alarm`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(sourceName)
        });
    } catch (e) { console.error('Sync error:', e); }
}

async function fetchAlarms(apiUrl, since = null, page = 1, size = 10) {
    let url = new URL(apiUrl);
    if (since) {
        url.searchParams.append('since', since.toISOString());
    } else {
        url.searchParams.append('pageNumber', page);
        url.searchParams.append('pageSize', size);

        const sName = document.getElementById('filterSourceName').value;
        const sev = document.getElementById('filterSeverity').value;
        const from = document.getElementById('filterFromDate').value;
        const to = document.getElementById('filterToDate').value;

        if (sName) url.searchParams.append('sourceName', sName);
        if (sev) url.searchParams.append('severity', sev);
        if (from) url.searchParams.append('fromDate', from);
        if (to) url.searchParams.append('toDate', to);
    }

    const response = await fetch(url.toString());
    if (!response.ok) throw new Error(`Status: ${response.status}`);
    return await response.json();
}

function applyFilters() {
    currentPage = 1;
    initialFetchAlarms();
}

function clearFilters() {
    document.querySelectorAll('.filters-card input, .filters-card select').forEach(el => el.value = '');
    applyFilters();
}

function exportToExcel() {
    const apiBase = window.location.origin;
    let url = new URL(`${apiBase}/api/alarms/export/excel`);

    const sName = document.getElementById('filterSourceName').value;
    const sev = document.getElementById('filterSeverity').value;
    const from = document.getElementById('filterFromDate').value;
    const to = document.getElementById('filterToDate').value;

    if (sName) url.searchParams.append('sourceName', sName);
    if (sev) url.searchParams.append('severity', sev);
    if (from) url.searchParams.append('fromDate', from);
    if (to) url.searchParams.append('toDate', to);

    window.location.href = url.toString();
}

function exportToPdf() {
    const apiBase = window.location.origin;
    let url = new URL(`${apiBase}/api/alarms/export/pdf`);

    const sName = document.getElementById('filterSourceName').value;
    const sev = document.getElementById('filterSeverity').value;
    const from = document.getElementById('filterFromDate').value;
    const to = document.getElementById('filterToDate').value;

    if (sName) url.searchParams.append('sourceName', sName);
    if (sev) url.searchParams.append('severity', sev);
    if (from) url.searchParams.append('fromDate', from);
    if (to) url.searchParams.append('toDate', to);

    window.location.href = url.toString();
}

function updateTable(alarms) {
    const tbody = document.getElementById('alarmBody');
    tbody.innerHTML = '';

    if (!alarms || alarms.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align: center; color: var(--text-muted); padding: 3rem;">No records found for current criteria.</td></tr>';
        return;
    }

    alarms.forEach(a => {
        const tr = document.createElement('tr');
        const date = a.eventTimeStamp ? new Date(a.eventTimeStamp).toLocaleString([], { dateStyle: 'short', timeStyle: 'medium' }) : '—';
        const sevClass = `sev-${a.severity || 1}`;

        tr.innerHTML = `
            <td style="font-size: 0.8125rem; color: var(--text-muted)">${date}</td>
            <td style="font-weight: 500">${a.message || '—'}</td>
            <td><span class="severity-tag ${sevClass}">${a.severity || 0}</span></td>
            <td style="font-weight: 600; color: var(--primary)">${a.sourceName || '—'}</td>
            <td style="color: var(--secondary)">${a.eventCategory || '—'}</td>
            <td>
                <span style="color: ${a.active ? 'var(--danger)' : 'var(--success)'}; font-weight: 600">
                    ${a.active ? 'ACTIVE' : 'RESOLVED'}
                </span>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

async function initialFetchAlarms() {
    const statusDiv = document.getElementById('status');
    const table = document.getElementById('alarmTable');
    const pagination = document.getElementById('pagination');

    try {
        const result = await fetchAlarms(`${window.location.origin}/api/alarms`, null, currentPage, currentPageSize);
        allAlarms = result.data;
        filteredAlarms = result.data;
        totalPages = result.totalPages;

        result.data.forEach(a => seenAlarmIds.add(a.eventID));

        statusDiv.style.display = 'none';
        table.style.display = 'table';
        pagination.style.display = 'flex';

        updateTable(filteredAlarms);
        updatePaginationInfo(result);
        lastCheckTime = new Date();
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    } catch (e) {
        statusDiv.innerHTML = `<div class="btn-danger" style="padding: 1rem; border-radius: 0.5rem">Error connecting to historian vault.</div>`;
    }
}

function updatePaginationInfo(result) {
    const start = result.totalCount === 0 ? 0 : (result.pageNumber - 1) * result.pageSize + 1;
    const end = Math.min(result.pageNumber * result.pageSize, result.totalCount);
    document.getElementById('paginationInfo').textContent = `Showing index ${start} - ${end} of ${result.totalCount} entries`;
    document.getElementById('prevBtn').disabled = !result.hasPreviousPage;
    document.getElementById('nextBtn').disabled = !result.hasNextPage;
}

function previousPage() { if (currentPage > 1) { currentPage--; initialFetchAlarms(); } }
function nextPage() { if (currentPage < totalPages) { currentPage++; initialFetchAlarms(); } }
function changePageSize(size) { currentPageSize = parseInt(size); currentPage = 1; initialFetchAlarms(); }

async function pollForNewAlarms(apiUrl) {
    try {
        if (!lastCheckTime) return;
        const newAlarms = await fetchAlarms(apiUrl, lastCheckTime);
        if (newAlarms.length > 0) {
            const trulyNew = newAlarms.filter(a => !seenAlarmIds.has(a.eventID));
            if (trulyNew.length > 0) {
                trulyNew.forEach(a => { addNewAlert(a); seenAlarmIds.add(a.eventID); });
                allAlarms = [...trulyNew, ...allAlarms];
                filteredAlarms = allAlarms;
                updateTable(filteredAlarms);
            }
        }
        lastCheckTime = new Date();
    } catch (e) { }
}

document.addEventListener('DOMContentLoaded', async () => {
    loadUnclearedAlarms();
    await initialFetchAlarms();
    setInterval(() => pollForNewAlarms(`${window.location.origin}/api/alarms`), POLL_INTERVAL);
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
});
