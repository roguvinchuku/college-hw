const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const { randomUUID } = require('node:crypto');

const dataFile = path.join(__dirname, 'parcels.json');
const parcels = fs.existsSync(dataFile)
    ? JSON.parse(fs.readFileSync(dataFile, 'utf8')) : [];
const port = Number(process.env.PORT || 8027);
const files = {
    '/': ['index.html', 'text/html'],
    '/index.html': ['index.html', 'text/html'],
    '/list.html': ['list.html', 'text/html'],
    '/style.css': ['style.css', 'text/css'],
    '/script.js': ['script.js', 'text/javascript'],
    '/list.js': ['list.js', 'text/javascript']
};

function sendJson(response, status, data) {
    response.writeHead(status, { 'Content-Type': 'application/json; charset=utf-8' });
    response.end(JSON.stringify(data));
}

const server = http.createServer(async (request, response) => {
    const pathname = new URL(request.url, 'http://localhost').pathname;

    if (pathname === '/PostService') {
        if (request.method === 'GET') {
            sendJson(response, 200, parcels);
            return;
        }
        if (request.method !== 'POST') {
            sendJson(response, 405, { error: 'Метод не підтримується.' });
            return;
        }

        let body = '';
        for await (const chunk of request) {
            body += chunk;
            if (body.length > 10000) {
                sendJson(response, 413, { error: 'Завеликий запит.' });
                return;
            }
        }

        let data;
        try {
            data = JSON.parse(body);
        } catch {
            sendJson(response, 400, { error: 'Некоректний JSON.' });
            return;
        }

        const validText = data && ['recipient', 'address'].every(name =>
            typeof data[name] === 'string' && data[name].trim() !== '');
        const validNumbers = data && ['weight', 'length', 'width', 'height'].every(name =>
            typeof data[name] === 'number' && Number.isFinite(data[name]) && data[name] > 0);
        if (!validText || !validNumbers) {
            sendJson(response, 400, { error: 'Заповніть отримувача й адресу та введіть додатні вагу й розміри.' });
            return;
        }

        const index = data.id ? parcels.findIndex(parcel => parcel.id === data.id) : -1;
        if (data.id && index === -1) {
            sendJson(response, 404, { error: 'Відправлення не знайдено.' });
            return;
        }
        const parcel = {
            id: data.id || randomUUID(),
            recipient: data.recipient.trim(),
            address: data.address.trim(),
            weight: data.weight,
            length: data.length,
            width: data.width,
            height: data.height
        };
        const updated = parcels.slice();
        if (index === -1) updated.push(parcel);
        else updated[index] = parcel;

        try {
            fs.writeFileSync(dataFile, JSON.stringify(updated, null, 2), 'utf8');
            parcels.splice(0, parcels.length, ...updated);
            sendJson(response, index === -1 ? 201 : 200, parcel);
        } catch {
            sendJson(response, 500, { error: 'Не вдалося записати дані на сервері.' });
        }
        return;
    }

    const file = files[pathname];
    if (!file || request.method !== 'GET') {
        response.writeHead(404);
        response.end('Not found');
        return;
    }
    response.writeHead(200, { 'Content-Type': file[1] + '; charset=utf-8' });
    fs.createReadStream(path.join(__dirname, file[0])).pipe(response);
});

server.listen(port, '127.0.0.1', () => {
    console.log(`Відкрийте http://127.0.0.1:${port}`);
});
