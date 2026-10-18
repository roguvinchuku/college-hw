async function loadParcels() {
    const message = document.getElementById('message');
    try {
        const response = await fetch('/PostService');
        if (!response.ok) throw new Error('Не вдалося завантажити список.');
        const parcels = await response.json();
        message.textContent = parcels.length === 0 ? 'Відправлень поки немає.' : '';

        for (const parcel of parcels) {
            const row = document.createElement('tr');
            for (const value of [parcel.recipient, parcel.address, parcel.weight,
                `${parcel.length} × ${parcel.width} × ${parcel.height}`]) {
                const cell = document.createElement('td');
                cell.textContent = value;
                row.appendChild(cell);
            }
            const action = document.createElement('td');
            const link = document.createElement('a');
            link.href = 'index.html?id=' + encodeURIComponent(parcel.id);
            link.textContent = 'Редагувати';
            action.appendChild(link);
            row.appendChild(action);
            document.getElementById('parcels').appendChild(row);
        }
    } catch (error) {
        message.textContent = error.message;
    }
}

loadParcels();
