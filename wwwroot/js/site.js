
// Global variables
let currentUser = null;
let notifications = [];

// Initialize the application
$(document).ready(function () {
    initializeApp();
    setupEventHandlers();
    loadNotifications();
});

// Application initialization
function initializeApp() {
    // Add loading animation to buttons
    $('form').on('submit', function () {
        const submitBtn = $(this).find('button[type="submit"]');
        const originalText = submitBtn.html();
        submitBtn.html('<span class="loading"></span> Loading...').prop('disabled', true);

        // Re-enable button after 10 seconds (fallback)
        setTimeout(() => {
            submitBtn.html(originalText).prop('disabled', false);
        }, 10000);
    });

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize date inputs
    $('input[type="date"]').each(function () {
        if (!$(this).val()) {
            const tomorrow = new Date();
            tomorrow.setDate(tomorrow.getDate() + 1);
            $(this).attr('min', tomorrow.toISOString().split('T')[0]);
        }
    });

    // Auto-refresh dashboard every 5 minutes
    if (window.location.pathname.includes('Dashboard')) {
        setInterval(refreshDashboard, 300000);
    }
}

// Event handlers setup
function setupEventHandlers() {
    // Appointment form validation
    $('#appointmentForm').on('submit', function (e) {
        if (!validateAppointmentForm()) {
            e.preventDefault();
            return false;
        }
    });

    // Real-time form validation
    $('.form-control').on('blur', function () {
        validateField($(this));
    });

    // Auto-save draft functionality
    $('textarea').on('input', function () {
        saveDraft($(this));
    });

    // Confirmation dialogs
    $('.confirm-action').on('click', function (e) {
        const message = $(this).data('confirm') || 'Are you sure?';
        if (!confirm(message)) {
            e.preventDefault();
            return false;
        }
    });

    // Search functionality
    $('#searchInput').on('keyup', function () {
        performSearch($(this).val());
    });

    // Mobile menu toggle
    $('.navbar-toggler').on('click', function () {
        $(this).toggleClass('active');
    });
}

// Form validation functions
function validateAppointmentForm() {
    let isValid = true;
    const requiredFields = ['DoctorId', 'AppointmentDate', 'AppointmentTime', 'Reason'];

    requiredFields.forEach(field => {
        const input = $(`[name="${field}"]`);
        if (!input.val()) {
            showFieldError(input, `${field.replace(/([A-Z])/g, ' $1').trim()} is required`);
            isValid = false;
        } else {
            clearFieldError(input);
        }
    });

    // Validate appointment date is not in the past
    const appointmentDate = new Date($('[name="AppointmentDate"]').val());
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    if (appointmentDate < today) {
        showFieldError($('[name="AppointmentDate"]'), 'Appointment date cannot be in the past');
        isValid = false;
    }

    return isValid;
}

function validateField(field) {
    const value = field.val().trim();
    const fieldName = field.attr('name');

    // Email validation
    if (field.attr('type') === 'email' && value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
            showFieldError(field, 'Please enter a valid email address');
            return false;
        }
    }

    // Phone validation
    if (fieldName === 'PhoneNumber' && value) {
        const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
        if (!phoneRegex.test(value)) {
            showFieldError(field, 'Please enter a valid phone number');
            return false;
        }
    }

    clearFieldError(field);
    return true;
}

function showFieldError(field, message) {
    field.addClass('is-invalid');
    let errorDiv = field.siblings('.invalid-feedback');
    if (errorDiv.length === 0) {
        errorDiv = $('<div class="invalid-feedback"></div>');
        field.after(errorDiv);
    }
    errorDiv.text(message);
}

function clearFieldError(field) {
    field.removeClass('is-invalid');
    field.siblings('.invalid-feedback').remove();
}

// Dashboard functions
function refreshDashboard() {
    // Only refresh if user is still on the page
    if (document.visibilityState === 'visible') {
        location.reload();
    }
}

// Search functionality
function performSearch(query) {
    if (query.length < 2) return;

    // Debounce search
    clearTimeout(window.searchTimeout);
    window.searchTimeout = setTimeout(() => {
        executeSearch(query);
    }, 500);
}

function executeSearch(query) {
    // Hide all searchable items
    $('.searchable-item').hide();

    // Show matching items
    $('.searchable-item').each(function () {
        const text = $(this).text().toLowerCase();
        if (text.includes(query.toLowerCase())) {
            $(this).show();
        }
    });

    // Show "no results" message if needed
    if ($('.searchable-item:visible').length === 0) {
        $('#noResults').show();
    } else {
        $('#noResults').hide();
    }
}

// Notification system
function loadNotifications() {
    // Simulate loading notifications
    // In a real app, this would be an AJAX call
    setTimeout(() => {
        showNotification('Welcome to SmartCare!', 'success');
    }, 2000);
}

function showNotification(message, type = 'info', duration = 5000) {
    const notification = $(`
        <div class="alert alert-${type} alert-dismissible fade show position-fixed" 
             style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
            <i class="fas fa-${getNotificationIcon(type)} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);

    $('body').append(notification);

    // Auto-dismiss after duration
    if (duration > 0) {
        setTimeout(() => {
            notification.alert('close');
        }, duration);
    }
}

function getNotificationIcon(type) {
    const icons = {
        'success': 'check-circle',
        'danger': 'exclamation-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// Auto-save functionality
function saveDraft(textarea) {
    const key = `draft_${textarea.attr('name')}_${window.location.pathname}`;
    localStorage.setItem(key, textarea.val());

    // Show saved indicator
    let indicator = textarea.siblings('.draft-saved');
    if (indicator.length === 0) {
        indicator = $('<small class="draft-saved text-muted">Draft saved</small>');
        textarea.after(indicator);
    }

    indicator.fadeIn().delay(2000).fadeOut();
}

function loadDraft(textarea) {
    const key = `draft_${textarea.attr('name')}_${window.location.pathname}`;
    const draft = localStorage.getItem(key);
    if (draft) {
        textarea.val(draft);
    }
}

// Utility functions
function formatDate(date) {
    return new Date(date).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

function formatTime(time) {
    const [hours, minutes] = time.split(':');
    const hour = parseInt(hours);
    const ampm = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour % 12 || 12;
    return `${displayHour}:${minutes} ${ampm}`;
}

function getStatusColor(status) {
    const colors = {
        'Scheduled': 'primary',
        'Confirmed': 'info',
        'InProgress': 'warning',
        'Completed': 'success',
        'Cancelled': 'danger'
    };
    return colors[status] || 'secondary';
}

// Export functions for global use
window.SmartCare = {
    showNotification,
    validateField,
    formatDate,
    formatTime,
    getStatusColor
};

// Handle offline/online status
window.addEventListener('online', function () {
    showNotification('Connection restored', 'success');
});

window.addEventListener('offline', function () {
    showNotification('You are currently offline', 'warning', 0);
});

// Print functionality
function printAppointmentDetails() {
    window.print();
}

// Export data functionality
function exportToCSV(data, filename) {
    const csv = convertToCSV(data);
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
}

function convertToCSV(data) {
    if (!data || data.length === 0) return '';

    const headers = Object.keys(data[0]);
    const csvHeaders = headers.join(',');
    const csvRows = data.map(row =>
        headers.map(header => {
            const value = row[header];
            return typeof value === 'string' ? `"${value.replace(/"/g, '""')}"` : value;
        }).join(',')
    );

    return [csvHeaders, ...csvRows].join('\n');
}

// Accessibility enhancements
$(document).ready(function () {
    // Add keyboard navigation
    $('.card').attr('tabindex', '0');

    // Enhance focus visibility
    $('*').on('focus', function () {
        $(this).addClass('focused');
    }).on('blur', function () {
        $(this).removeClass('focused');
    });

    // Skip to main content link
    if ($('#main-content').length === 0) {
        $('main').attr('id', 'main-content');
    }
});