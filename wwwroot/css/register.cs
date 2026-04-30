/* تنسيقات عامة */
.input - validation - error {
    border - color: #dc3545 !important;
    background - color: #fff8f8 !important;
}

.field - validation - error, .text - danger {
    font - size: 0.875rem;
    margin - top: 0.25rem;
}

/* تنسيق حقول الدكتور */
# doctorFields {
transition: all 0.3s ease;
}

.clinic - row {
position: relative;
transition: all 0.2s ease;
}

.clinic - row:hover {
    background-color: #f8f9fa;
    border - radius: 0.375rem;
padding: 0.5rem;
margin: 0 - 0.5rem 0.5rem -0.5rem;
}

.remove - clinic {
    margin - top: 0.5rem;
}

/* تنسيق الأزرار */
.btn - success {
background: linear - gradient(135deg, #28a745 0%, #20c997 100%);
    border: none;
transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.btn - success:hover {
    transform: translateY(-2px);
box - shadow: 0 5px 15px rgba(40, 167, 69, 0.3);
}

/* تنسيق الفورم */
.form - control:focus {
    border-color: #86b7fe;
    box - shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
}

/* responsive */
@media(max - width: 768px) {
    .container {
    padding: 0 1rem;
    }
    
    .col - md - 6 {
    padding: 1rem!important;
    }
}