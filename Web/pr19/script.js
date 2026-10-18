function isValidGrade(grade) {
    return typeof grade === 'number' && Number.isFinite(grade)
        && grade >= 0 && grade <= 100;
}

function calculateAverage(grades) {
    const result = document.getElementById('result');
    document.getElementById('grades').textContent = JSON.stringify(grades);

    if (!Array.isArray(grades) || grades.length === 0) {
        result.textContent = 'Помилка: потрібно передати непорожній масив оцінок.';
        return null;
    }

    let sum = 0;

    for (const grade of grades) {
        if (!isValidGrade(grade)) {
            result.textContent = 'Помилка: кожна оцінка має бути числом від 0 до 100.';
            return null;
        }

        sum += grade;
    }

    const average = sum / grades.length;
    result.textContent = 'Середня оцінка: ' + average;
    return average;
}

let studentGrades = [85, 90, 78, 92, 88, 'A'];
calculateAverage(studentGrades);
