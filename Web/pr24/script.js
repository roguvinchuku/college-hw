class Parcel {
    #weight;
    static #count = 0;

    constructor(recipient, weight, length, width, height) {
        this.recipient = recipient;
        this.weight = weight;
        this.length = length;
        this.width = width;
        this.height = height;
        Parcel.#count++;
    }

    get weight() {
        return this.#weight;
    }

    set weight(value) {
        if (typeof value !== 'number' || !Number.isFinite(value) || value <= 0) {
            throw new Error('Вага посилки має бути додатним числом.');
        }
        this.#weight = value;
    }

    get volumetricWeight() {
        return this.length * this.width * this.height / 5000;
    }

    get chargeableWeight() {
        return Math.max(this.weight, this.volumetricWeight);
    }

    describe() {
        return `Посилка для ${this.recipient}, вага ${this.weight} кг, `
            + `розміри ${this.length} × ${this.width} × ${this.height} см, `
            + `об'ємна вага ${this.volumetricWeight} кг, `
            + `розрахункова вага ${this.chargeableWeight} кг`;
    }

    static getCount() {
        return Parcel.#count;
    }
}

class ParcelPriceCalculator {
    constructor(ratePerKg) {
        this.ratePerKg = ratePerKg;
    }

    calculate(parcel) {
        return parcel.chargeableWeight * this.ratePerKg;
    }

    getName() {
        return 'Базовий тариф';
    }
}

class PostOfficeParcelPriceCalculator extends ParcelPriceCalculator {
    constructor() {
        super(15);
    }

    getName() {
        return 'Відділення пошти';
    }
}

class CourierParcelPriceCalculator extends ParcelPriceCalculator {
    constructor() {
        super(30);
    }

    getName() {
        return "Кур'єрська доставка";
    }
}

class ExpressParcelPriceCalculator extends ParcelPriceCalculator {
    constructor() {
        super(30);
        this.expressRatePerKg = 25;
    }

    calculate(parcel) {
        return super.calculate(parcel) + parcel.chargeableWeight * this.expressRatePerKg;
    }

    getName() {
        return 'Експрес-доставка';
    }
}

const parcels = [
    new Parcel('Іван Петренко', 2.5, 40, 30, 20),
    new Parcel('Олена Коваль', 5, 10, 10, 10)
];

const calculators = [
    new PostOfficeParcelPriceCalculator(),
    new CourierParcelPriceCalculator(),
    new ExpressParcelPriceCalculator()
];

for (const parcel of parcels) {
    console.log(parcel.describe());
    for (const calculator of calculators) {
        console.log(calculator.getName() + ': ' + calculator.calculate(parcel) + ' грн');
    }
}

console.log('Кількість створених відправлень:', Parcel.getCount());
