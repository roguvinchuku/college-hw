const form = document.getElementById('parcel-form');
const fields = Array.from(form.querySelectorAll('input'));
const message = document.getElementById('message');
const saveButton = document.getElementById('save');
let parcelId = new URLSearchParams(window.location.search).get('id');

function clearError(field) {
    field.classList.remove('invalid');
    field.removeAttribute('aria-invalid');
    document.getElementById(field.id + '-error').textContent = '';
}

function validateField(field) {
    const value = field.value.trim();
    let error = '';

    if (value === '') {
        error = 'Заповніть це поле.';
    } else if (field.type === 'number'
        && (!Number.isFinite(Number(value)) || Number(value) <= 0)) {
        error = 'Введіть число більше нуля.';
    }

    clearError(field);
    if (error) {
        field.classList.add('invalid');
        field.setAttribute('aria-invalid', 'true');
        document.getElementById(field.id + '-error').textContent = error;
    }
    return error === '';
}

for (const field of fields) {
    field.addEventListener('change', () => validateField(field));
    field.addEventListener('input', () => {
        clearError(field);
        message.textContent = '';
    });
}

async function saveParcel(event) {
    event.preventDefault();
    if (saveButton.disabled) return;
    message.textContent = '';

    let valid = true;
    for (const field of fields) {
        if (!validateField(field)) valid = false;
    }
    if (!valid) return;

    const parcel = {};
    for (const field of fields) {
        parcel[field.name] = field.type === 'number'
            ? Number(field.value) : field.value.trim();
    }
    if (parcelId) parcel.id = parcelId;

    saveButton.disabled = true;
    try {
        const response = await fetch('/PostService', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(parcel)
        });
        const data = await response.json();
        if (!response.ok) throw new Error(data.error || 'Не вдалося зберегти відправлення.');
        parcelId = data.id;
        window.history.replaceState(null, '', '?id=' + encodeURIComponent(parcelId));
        message.textContent = 'Відправлення збережено.';
    } catch (error) {
        message.textContent = 'Помилка збереження: ' + error.message;
    } finally {
        saveButton.disabled = false;
    }
}

saveButton.addEventListener('click', saveParcel);
form.addEventListener('submit', saveParcel);
document.getElementById('cancel').addEventListener('click', () => {
    window.location.href = 'list.html';
});

async function loadParcel() {
    if (!parcelId) return;
    saveButton.disabled = true;
    try {
        const response = await fetch('/PostService');
        if (!response.ok) throw new Error('Не вдалося завантажити відправлення.');
        const parcels = await response.json();
        const parcel = parcels.find(item => item.id === parcelId);
        if (!parcel) throw new Error('Відправлення не знайдено.');
        for (const field of fields) field.value = parcel[field.name];
        saveButton.disabled = false;
    } catch (error) {
        message.textContent = error.message;
    }
}

loadParcel();
