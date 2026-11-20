// ============================================
// Global State
// ============================================

let allClasses = [];
let customClasses = [];
let allPrograms = [];

// Time slots for the timetable (90-minute slots)
const TIME_SLOTS = [
    '07:00-07:50', '07:50-08:40', '08:40-09:30', '09:30-10:20',
    '10:20-11:10', '11:10-12:00', '12:00-12:50', '12:50-13:40',
    '13:40-14:30', '14:30-15:20', '15:20-16:10', '16:10-17:00',
    '17:00-17:50', '17:50-18:40', '18:40-19:30', '19:30-20:20'
];

const DAYS = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday'];

// ============================================
// Initialization
// ============================================

document.addEventListener('DOMContentLoaded', () => {
    initializeUI();
    loadStudyData();
    setupEventListeners();
    initializeTimetable();
});

// ============================================
// Setup
// ============================================

function initializeUI() {
    // Populate study programs checkboxes
    const studyProgramsContainer = document.getElementById('study-programs');
    const programs = ['BIT', 'MIT', 'MIT-NBIO', 'MIT-NISD', 'MIT-NISY'];
    
    programs.forEach(program => {
        const label = document.createElement('label');
        const input = document.createElement('input');
        input.type = 'checkbox';
        input.value = program;
        input.checked = true;
        label.appendChild(input);
        label.appendChild(document.createTextNode(` ${program}`));
        studyProgramsContainer.appendChild(label);
    });

    // Initialize timetable
    initializeTimetable();
}

function setupEventListeners() {
    // Tab switching
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            const tabName = btn.dataset.tab;
            switchTab(tabName);
        });
    });

    // Load button
    document.getElementById('load-btn').addEventListener('click', loadStudyData);

    // Add class button
    document.getElementById('add-class-btn').addEventListener('click', addCustomClass);

    // Export buttons
    document.getElementById('export-json').addEventListener('click', exportToJSON);
    document.getElementById('export-calendar').addEventListener('click', exportToCalendar);

    // Filters
    document.getElementById('semester-select').addEventListener('change', applyFilters);
    document.querySelectorAll('input[name="semester-type"]').forEach(radio => {
        radio.addEventListener('change', applyFilters);
    });
    document.getElementById('required-only').addEventListener('change', applyFilters);
    document.getElementById('elective-only').addEventListener('change', applyFilters);
    document.getElementById('selected-only').addEventListener('change', applyFilters);
}

function initializeTimetable() {
    const tbody = document.getElementById('timetable-body');
    tbody.innerHTML = '';

    TIME_SLOTS.forEach(slot => {
        const [startTime] = slot.split('-');
        const row = document.createElement('tr');
        
        const timeCell = document.createElement('td');
        timeCell.className = 'time-col';
        timeCell.textContent = startTime;
        row.appendChild(timeCell);

        DAYS.forEach(day => {
            const cell = document.createElement('td');
            cell.className = `day-cell ${day.toLowerCase()}`;
            cell.dataset.day = day;
            cell.dataset.time = startTime;
            row.appendChild(cell);
        });

        tbody.appendChild(row);
    });
}

// ============================================
// Data Loading
// ============================================

async function loadStudyData() {
    try {
        const response = await fetch('/bojosh');
        const text = await response.text();
        
        // Parse the response
        const lines = text.trim().split('\n');
        allPrograms = [];
        allClasses = [];

        let parsingClasses = false;

        lines.forEach((line, index) => {
            if (line.trim() === '') {
                parsingClasses = true;
                return;
            }

            if (!parsingClasses) {
                // Parse programme line
                const parts = line.split(' - ');
                if (parts.length >= 4) {
                    allPrograms.push({
                        name: parts[0].trim(),
                        code: extractCode(parts[0]),
                        type: parts[2]?.trim() || 'Unknown',
                        duration: parts[1]?.trim() || '',
                        studyType: parts[3]?.trim() || '',
                        isEnglish: line.includes('English')
                    });
                }
            } else {
                // Parse class line
                const parts = line.split(' - ');
                if (parts.length >= 4) {
                    allClasses.push({
                        name: parts[0].trim(),
                        code: extractCode(parts[0]),
                        type: parts[2]?.trim() || 'Lecture',
                        semester: parts[3]?.trim() || 'Winter',
                        credits: parts[4]?.trim() || '0',
                        day: getRandomDay(),
                        startTime: getRandomTime(),
                        room: 'TBA',
                        instructor: 'TBA',
                        color: getRandomColor()
                    });
                }
            }
        });

        // Update UI
        updateScheduleCount();
        populateProgramsList();
        displayClasses();

        console.log(`Loaded ${allPrograms.length} programmes and ${allClasses.length} classes`);
    } catch (error) {
        console.error('Error loading study data:', error);
        alert('Error loading study data. Make sure the backend is running.');
    }
}

function extractCode(text) {
    const match = text.match(/\(([^)]+)\)/);
    return match ? match[1] : 'Unknown';
}

function getRandomDay() {
    return DAYS[Math.floor(Math.random() * DAYS.length)];
}

function getRandomTime() {
    const times = ['07:00', '08:00', '09:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00'];
    return times[Math.floor(Math.random() * times.length)];
}

function getRandomColor() {
    const colors = ['green', 'blue', 'red', 'orange', 'purple'];
    return colors[Math.floor(Math.random() * colors.length)];
}

// ============================================
// Display Functions
// ============================================

function displayClasses() {
    const tbody = document.getElementById('timetable-body');
    
    // Clear existing class blocks
    document.querySelectorAll('.class-block').forEach(block => block.remove());

    // Add classes and custom classes
    const allDisplayClasses = [...allClasses, ...customClasses];

    allDisplayClasses.forEach(classItem => {
        const cells = document.querySelectorAll(`td[data-day="${classItem.day}"][data-time="${classItem.startTime}"]`);
        cells.forEach(cell => {
            const classBlock = document.createElement('div');
            classBlock.className = `class-block ${classItem.color}`;
            classBlock.innerHTML = `
                <strong>${classItem.name}</strong><br>
                <small>${classItem.startTime}</small>
            `;
            classBlock.title = `${classItem.name} - ${classItem.room} - ${classItem.instructor}`;
            classBlock.addEventListener('click', () => editClass(classItem));
            cell.appendChild(classBlock);
        });
    });
}

function populateProgramsList() {
    const container = document.getElementById('programs-list');
    container.innerHTML = '';

    allPrograms.slice(0, 10).forEach(program => {
        const card = document.createElement('div');
        card.className = 'program-card';
        card.innerHTML = `
            <h4>${program.name}</h4>
            <p><strong>Code:</strong> ${program.code}</p>
            <p><strong>Type:</strong> ${program.type}</p>
            <p><strong>Duration:</strong> ${program.duration}</p>
            <p><strong>Study Type:</strong> ${program.studyType}</p>
            <div>
                ${program.isEnglish ? '<span class="badge">English</span>' : ''}
            </div>
        `;
        container.appendChild(card);
    });
}

function updateScheduleCount() {
    const count = allClasses.length + customClasses.length;
    document.getElementById('schedule-count').textContent = `${count}/${allClasses.length + customClasses.length}`;
}

// ============================================
// Tab Switching
// ============================================

function switchTab(tabName) {
    // Update tab buttons
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    document.querySelector(`[data-tab="${tabName}"]`).classList.add('active');

    // Update tab content
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });
    document.getElementById(`${tabName}-tab`).classList.add('active');

    // Load content for specific tabs
    if (tabName === 'summary') {
        populateProgramsList();
    }
}

// ============================================
// Class Management
// ============================================

function addCustomClass() {
    const day = document.getElementById('class-day').value;
    const startTime = document.getElementById('class-start').value;
    const endTime = document.getElementById('class-end').value;
    const name = document.getElementById('class-name').value;
    const room = document.getElementById('class-location').value;
    const instructor = document.getElementById('class-instructor').value;
    const color = document.getElementById('class-color').value;

    if (!name || !room) {
        alert('Please fill in the class name and room!');
        return;
    }

    const customClass = {
        name,
        code: 'CUSTOM',
        type: 'Custom',
        day,
        startTime,
        endTime,
        room,
        instructor: instructor || 'TBA',
        color,
        isCustom: true
    };

    customClasses.push(customClass);
    updateScheduleCount();
    displayClasses();

    // Clear form
    document.getElementById('class-name').value = '';
    document.getElementById('class-location').value = '';
    document.getElementById('class-instructor').value = '';

    alert('Class added successfully!');
}

function editClass(classItem) {
    alert(`Editing: ${classItem.name}\n\nFull details:\nRoom: ${classItem.room}\nInstructor: ${classItem.instructor}`);
}

// ============================================
// Filtering
// ============================================

function applyFilters() {
    const semester = document.getElementById('semester-select').value;
    const semesterType = document.querySelector('input[name="semester-type"]:checked').value;
    const requiredOnly = document.getElementById('required-only').checked;
    const electiveOnly = document.getElementById('elective-only').checked;
    const selectedOnly = document.getElementById('selected-only').checked;

    // Filter classes based on criteria
    let filteredClasses = allClasses.filter(classItem => {
        if (requiredOnly && classItem.type !== 'Required') return false;
        if (electiveOnly && classItem.type !== 'Elective') return false;
        if (selectedOnly && !classItem.selected) return false;
        return true;
    });

    // Display filtered classes
    displayClasses();
}

// ============================================
// Export Functions
// ============================================

function exportToJSON() {
    const data = {
        semester: document.getElementById('semester-select').value,
        semesterType: document.querySelector('input[name="semester-type"]:checked').value,
        programs: allPrograms,
        classes: [...allClasses, ...customClasses],
        exportDate: new Date().toISOString()
    };

    const jsonString = JSON.stringify(data, null, 2);
    const blob = new Blob([jsonString], { type: 'application/json' });
    downloadFile(blob, 'schedule.json');
}

function exportToCalendar() {
    let icsContent = `BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Bojosh//FIT Scheduler//EN
CALSCALE:GREGORIAN
METHOD:PUBLISH
X-WR-CALNAME:FIT Study Schedule
X-WR-TIMEZONE:Europe/Prague
`;

    const allDisplayClasses = [...allClasses, ...customClasses];
    
    allDisplayClasses.forEach((classItem, index) => {
        const eventDate = getNextOccurrenceDate(classItem.day);
        const startTime = timeStringToDateTime(eventDate, classItem.startTime);
        const endTime = timeStringToDateTime(eventDate, classItem.endTime || classItem.startTime);

        icsContent += `
BEGIN:VEVENT
UID:bojosh-${index}-${Date.now()}@bojosh.local
DTSTAMP:${new Date().toISOString().replace(/[-:]/g, '').split('.')[0]}Z
DTSTART:${formatDateTimeForICS(startTime)}
DTEND:${formatDateTimeForICS(endTime)}
SUMMARY:${classItem.name} (${classItem.type})
LOCATION:${classItem.room}
DESCRIPTION:${classItem.instructor}
RRULE:FREQ=WEEKLY;COUNT=13
END:VEVENT`;
    });

    icsContent += `
END:VCALENDAR`;

    const blob = new Blob([icsContent], { type: 'text/calendar' });
    downloadFile(blob, 'schedule.ics');
}

function getNextOccurrenceDate(dayName) {
    const dayIndex = DAYS.indexOf(dayName);
    const today = new Date();
    const currentDay = today.getDay();
    
    let daysAhead = dayIndex - (currentDay - 1);
    if (daysAhead <= 0) daysAhead += 7;
    
    const result = new Date(today);
    result.setDate(result.getDate() + daysAhead);
    return result;
}

function timeStringToDateTime(date, timeString) {
    const [hours, minutes] = timeString.split(':');
    const result = new Date(date);
    result.setHours(parseInt(hours), parseInt(minutes), 0, 0);
    return result;
}

function formatDateTimeForICS(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    const seconds = String(date.getSeconds()).padStart(2, '0');
    
    return `${year}${month}${day}T${hours}${minutes}${seconds}`;
}

function downloadFile(blob, filename) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}

