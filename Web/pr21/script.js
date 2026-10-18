(() => {
    function showResult(expression, value) {
        console.log(expression, value, typeof value);
    }

    showResult('"10" + 5:', "10" + 5);
    showResult('"10" - 5:', "10" - 5);
    showResult('"10" * "2":', "10" * "2");
    showResult('10 + true:', 10 + true);
    showResult('10 + false:', 10 + false);
    showResult('Number("125 грн"):', Number("125 грн"));
    showResult('Number(""):', Number(""));
    showResult('Boolean("false"):', Boolean("false"));
    showResult('Boolean(""):', Boolean(""));

    const products = [
        { name: 'Зошит', price: 40, quantity: 3 },
        { name: 'Ручка', price: 15, quantity: 4 },
        { name: 'Папка', price: 60, quantity: 2 }
    ];
    const vatRate = 0.2;
    const vatResults = [];

    function ItemCostCalculator(price, quantity) {
        return price * quantity;
    }

    function OrderSumCalculator(items, rate, taxes) {
        let total = 0;
        taxes.length = 0;

        for (const item of items) {
            const itemCost = ItemCostCalculator(item.price, item.quantity);
            const vat = Math.round(itemCost * rate * 100) / 100;
            taxes.push({ name: item.name, vat: vat });
            total += itemCost + vat;
        }

        return Math.round(total * 100) / 100;
    }

    const orderTotal = OrderSumCalculator(products, vatRate, vatResults);
    console.log('Загальна вартість замовлення з ПДВ:', orderTotal);
    console.log('ПДВ для кожного товару:', vatResults);
})();