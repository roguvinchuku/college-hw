const value1 = document.getElementById('value1');
const value2 = document.getElementById('value2');
const result = document.getElementById('result');
const historyList = document.getElementById('history');

function calculate() {
    const a = Number(value1.value);
    const b = Number(value2.value);
    const sum = a + b;

    result.textContent = sum;

    const historyItem = document.createElement('li');
    historyItem.textContent = `${a} + ${b} = ${sum}`;
    historyList.appendChild(historyItem);
}
