const apiBase = window.location.origin;
const tagsApiUrl = `${apiBase}/api/tags`;

let currentEditId = null;
let currentPage = 1;
let currentPageSize = 10;
let totalPages = 1;

async function fetchTags() {
    const statusDiv = document.getElementById('status');
    const table = document.getElementById('tagTable');
    const tbody = document.getElementById('tagBody');
    const pagination = document.getElementById('pagination');

    try {
        const response = await fetch(`${tagsApiUrl}?pageNumber=${currentPage}&pageSize=${currentPageSize}`);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

        const result = await response.json();

        if (result.totalCount === 0) {
            statusDiv.innerHTML = `
                <div class="loading-state">
                    <p>No system tags found.</p>
                    <button class="btn btn-outline" style="margin-top: 1rem" onclick="showAddTagModal()">Create First Tag</button>
                </div>`;
            table.style.display = 'none';
            pagination.style.display = 'none';
            return;
        }

        statusDiv.style.display = 'none';
        table.style.display = 'table';
        pagination.style.display = 'flex';
        tbody.innerHTML = '';

        totalPages = result.totalPages;
        updatePaginationInfo(result);

        result.data.forEach(tag => {
            const tr = document.createElement('tr');
            tr.setAttribute('data-tag-name', tag.tagName);
            const updatedDate = tag.updatedAt ? new Date(tag.updatedAt).toLocaleDateString() + ' ' + new Date(tag.updatedAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : 'N/A';
            const statusBadge = tag.isActive
                ? '<span class="status-badge status-active">Active</span>'
                : '<span class="status-badge status-inactive">Inactive</span>';

            tr.innerHTML = `
                <td>
                    <div style="font-weight: 600; color: var(--primary)">${tag.tagName || ''}</div>
                    <div style="font-size: 0.75rem; color: var(--text-muted)">ID: #${tag.id}</div>
                </td>
                <td><span style="color: var(--text-muted)">${tag.description || '—'}</span></td>
                <td><code class="live-value" id="val-${tag.tagName}">...</code></td>
                <td><code>${tag.highLimit}</code></td>
                <td><code>${tag.lowLimit}</code></td>
                <td>${statusBadge}</td>
                <td style="color: var(--text-muted); font-size: 0.75rem">${updatedDate}</td>
                <td style="text-align: right">
                    <div class="action-btns" style="justify-content: flex-end">
                        <button class="icon-btn" title="Edit Tag" onclick="editTag(${tag.id})">
                            <i data-lucide="edit-3" style="width: 16px; height: 16px;"></i>
                        </button>
                        <button class="icon-btn delete" title="Delete Tag" onclick="deleteTag(${tag.id})">
                            <i data-lucide="trash-2" style="width: 16px; height: 16px;"></i>
                        </button>
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
        pollLiveValues(); // Initial poll after table loads
    } catch (error) {
        console.error('Error fetching tags:', error);
        statusDiv.innerHTML = '<div class="error-message">Unable to connect to the monitoring service. Please ensure the API is active.</div>';
        table.style.display = 'none';
        pagination.style.display = 'none';
    }
}

function updatePaginationInfo(result) {
    const start = (result.pageNumber - 1) * result.pageSize + 1;
    const end = Math.min(result.pageNumber * result.pageSize, result.totalCount);
    document.getElementById('paginationInfo').textContent = `Showing ${start} to ${end} of ${result.totalCount}`;

    document.getElementById('prevBtn').disabled = !result.hasPreviousPage;
    document.getElementById('nextBtn').disabled = !result.hasNextPage;
}

function previousPage() {
    if (currentPage > 1) {
        currentPage--;
        fetchTags();
    }
}

function nextPage() {
    if (currentPage < totalPages) {
        currentPage++;
        fetchTags();
    }
}

function changePageSize(size) {
    currentPageSize = parseInt(size);
    currentPage = 1;
    fetchTags();
}

function showAddTagModal() {
    currentEditId = null;
    document.getElementById('tagForm').reset();
    document.querySelector('.modal-header').textContent = 'Create New System Tag';
    document.getElementById('tagModal').style.display = 'flex';
}

function closeModal() {
    document.getElementById('tagModal').style.display = 'none';
}

async function editTag(id) {
    try {
        const response = await fetch(`${tagsApiUrl}/${id}`);
        const tag = await response.json();

        currentEditId = id;
        document.getElementById('tagName').value = tag.tagName;
        document.getElementById('description').value = tag.description || '';
        document.getElementById('highLimit').value = tag.highLimit;
        document.getElementById('lowLimit').value = tag.lowLimit;

        document.querySelector('.modal-header').textContent = 'Update Tag Configuration';
        document.getElementById('tagModal').style.display = 'flex';
    } catch (error) {
        alert('Error loading tag details');
    }
}

async function deleteTag(id) {
    if (!confirm('Are you certain you want to remove this tag from the system?')) return;

    try {
        const response = await fetch(`${tagsApiUrl}/${id}`, { method: 'DELETE' });
        if (response.ok) {
            fetchTags();
        } else {
            alert('Request failed');
        }
    } catch (error) {
        alert('Network error during deletion');
    }
}

document.getElementById('tagForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const tagData = {
        tagName: document.getElementById('tagName').value,
        description: document.getElementById('description').value,
        highLimit: parseFloat(document.getElementById('highLimit').value),
        lowLimit: parseFloat(document.getElementById('lowLimit').value),
        isActive: true
    };

    try {
        let response;
        if (currentEditId) {
            tagData.id = currentEditId;
            response = await fetch(`${tagsApiUrl}/${currentEditId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(tagData)
            });
        } else {
            response = await fetch(tagsApiUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(tagData)
            });
        }

        if (response.ok) {
            closeModal();
            fetchTags();
        } else {
            alert('Error saving configuration');
        }
    } catch (error) {
        alert('Connection failure');
    }
});

async function pollLiveValues() {
    const liveCells = document.querySelectorAll('.live-value');
    liveCells.forEach(async (cell) => {
        const tagName = cell.id.replace('val-', '');
        try {
            const response = await fetch(`${apiBase}/api/livetags/${tagName}`);
            if (response.ok) {
                const result = await response.json();
                cell.textContent = result.value;

                // Add a small highlight effect on change
                cell.style.color = '#3b82f6';
                setTimeout(() => cell.style.color = '', 500);
            }
        } catch (e) {
            cell.textContent = 'Err';
        }
    });
}

async function checkGlobalAlerts() {
    try {
        const response = await fetch(`${apiBase}/api/alarms/unacknowledged/count`);
        if (response.ok) {
            const count = await response.json();
            if (count > 0) {
                document.body.classList.add('alert-active-blink');
                if (window.alarmSound) window.alarmSound.start();
            } else {
                document.body.classList.remove('alert-active-blink');
                if (window.alarmSound) window.alarmSound.stop();
            }
        }
    } catch (e) {
        console.error('Failed to check global alerts:', e);
    }
}

function refreshTags() {
    fetchTags();
}

document.addEventListener('DOMContentLoaded', () => {
    fetchTags();
    setInterval(pollLiveValues, 5000); // Poll live values every 5 seconds
    setInterval(checkGlobalAlerts, 2000); // Check for global alerts every 2 seconds
    checkGlobalAlerts(); // Initial check
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
});

window.onclick = function (event) {
    const modal = document.getElementById('tagModal');
    if (event.target == modal) closeModal();
}
