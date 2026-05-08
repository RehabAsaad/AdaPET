// ✅ نسخة معدلة - التحقق فقط عند ترك الحقل (blur) وليس أثناء الكتابة
let clinicCounter = 1;

// دالة التحقق من صحة النموذج الأساسي (لحقل واحد فقط)
window.validateSingleField = function (fieldId) {
    let isValid = true;

    switch (fieldId) {
        case 'Name':
            const name = $('#Name').val().trim();
            const nameErrorSpan = $('#Name').siblings('.text-danger');
            if (!name) {
                nameErrorSpan.text('Full name is required');
                $('#Name').addClass('input-validation-error');
                isValid = false;
            } else if (name.length < 3) {
                nameErrorSpan.text('⚠️ Name must be at least 3 characters');
                $('#Name').addClass('input-validation-error');
                isValid = false;
            } else if (name.length > 50) {
                nameErrorSpan.text('⚠️ Name cannot exceed 50 characters');
                $('#Name').addClass('input-validation-error');
                isValid = false;
            } else {
                nameErrorSpan.text('');
                $('#Name').removeClass('input-validation-error');
            }
            break;

        case 'Email':
            const email = $('#Email').val().trim();
            const emailErrorSpan = $('#Email').siblings('.text-danger');
            const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
            if (!email) {
                emailErrorSpan.text('Email is required');
                $('#Email').addClass('input-validation-error');
                isValid = false;
            } else if (!emailRegex.test(email)) {
                emailErrorSpan.text('⚠️ Please enter a valid email address');
                $('#Email').addClass('input-validation-error');
                isValid = false;
            } else if (email.length > 100) {
                emailErrorSpan.text('Email cannot exceed 100 characters');
                $('#Email').addClass('input-validation-error');
                isValid = false;
            } else {
                emailErrorSpan.text('');
                $('#Email').removeClass('input-validation-error');
            }
            break;

        case 'Phone':
            const phone = $('#Phone').val().trim();
            const phoneErrorSpan = $('#Phone').siblings('.text-danger');
            const phoneRegex = /^01[0125][0-9]{8}$/;
            if (!phone) {
                phoneErrorSpan.text('Phone number is required');
                $('#Phone').addClass('input-validation-error');
                isValid = false;
            } else if (!phoneRegex.test(phone)) {
                phoneErrorSpan.text('⚠️ Please enter a valid Egyptian phone number');
                $('#Phone').addClass('input-validation-error');
                isValid = false;
            } else {
                phoneErrorSpan.text('');
                $('#Phone').removeClass('input-validation-error');
            }
            break;

        case 'Password':
            const password = $('#Password').val();
            const passwordErrorSpan = $('#Password').siblings('.text-danger');
            if (!password) {
                passwordErrorSpan.text('Password is required');
                $('#Password').addClass('input-validation-error');
                isValid = false;
            } else if (password.length < 6) {
                passwordErrorSpan.text('⚠️ Password must be at least 6 characters');
                $('#Password').addClass('input-validation-error');
                isValid = false;
            } else if (password.length > 100) {
                passwordErrorSpan.text('Password cannot exceed 100 characters');
                $('#Password').addClass('input-validation-error');
                isValid = false;
            } else {
                passwordErrorSpan.text('');
                $('#Password').removeClass('input-validation-error');
            }
            break;

        case 'ConfirmPassword':
            const confirmPassword = $('#ConfirmPassword').val();
            const confirmErrorSpan = $('#ConfirmPassword').siblings('.text-danger');
            const mainPassword = $('#Password').val();
            if (!confirmPassword) {
                confirmErrorSpan.text('Please confirm your password');
                $('#ConfirmPassword').addClass('input-validation-error');
                isValid = false;
            } else if (mainPassword !== confirmPassword) {
                confirmErrorSpan.text('⚠️ Passwords do not match');
                $('#ConfirmPassword').addClass('input-validation-error');
                isValid = false;
            } else {
                confirmErrorSpan.text('');
                $('#ConfirmPassword').removeClass('input-validation-error');
            }
            break;
    }

    return isValid;
};

// دالة التحقق من جميع الحقول الأساسية (للـ Submit)
window.validateAllBasicFields = function () {
    let isValid = true;

    if (!window.validateSingleField('Name')) isValid = false;
    if (!window.validateSingleField('Email')) isValid = false;
    if (!window.validateSingleField('Phone')) isValid = false;
    if (!window.validateSingleField('Password')) isValid = false;
    if (!window.validateSingleField('ConfirmPassword')) isValid = false;

    return isValid;
};

// دالة التحقق من صحة حقول الدكتور
window.validateDoctorFields = function () {
    let isValid = true;

    if ($('#roleSelect').val() !== 'Doctor') {
        return true;
    }

    console.log("Validating doctor fields...");

    // 1. التحقق من التخصص
    const specialization = $('#Specialization').val().trim();
    if (!specialization) {
        $('#specializationError').text('Specialization is required');
        $('#Specialization').addClass('input-validation-error');
        isValid = false;
    } else if (specialization.length < 3) {
        $('#specializationError').text('Specialization must be at least 3 characters');
        $('#Specialization').addClass('input-validation-error');
        isValid = false;
    } else if (specialization.length > 100) {
        $('#specializationError').text('Specialization cannot exceed 100 characters');
        $('#Specialization').addClass('input-validation-error');
        isValid = false;
    } else {
        $('#specializationError').text('');
        $('#Specialization').removeClass('input-validation-error');
    }

    // 2. التحقق من العيادات
    let hasValidClinic = false;
    let allClinicsValid = true;

    $('.clinic-row').each(function (index) {
        const clinicName = $(this).find('.clinic-name').val().trim();
        const clinicAddress = $(this).find('.clinic-address').val().trim();
        const nameErrorSpan = $(this).find('.clinic-name-error');
        const addressErrorSpan = $(this).find('.clinic-address-error');

        let rowValid = true;

        if (!clinicName) {
            nameErrorSpan.text('Clinic name is required');
            $(this).find('.clinic-name').addClass('input-validation-error');
            rowValid = false;
            allClinicsValid = false;
        } else if (clinicName.length < 3) {
            nameErrorSpan.text('Clinic name must be at least 3 characters');
            $(this).find('.clinic-name').addClass('input-validation-error');
            rowValid = false;
            allClinicsValid = false;
        } else if (clinicName.length > 100) {
            nameErrorSpan.text('Clinic name cannot exceed 100 characters');
            $(this).find('.clinic-name').addClass('input-validation-error');
            rowValid = false;
            allClinicsValid = false;
        } else {
            nameErrorSpan.text('');
            $(this).find('.clinic-name').removeClass('input-validation-error');
        }

        if (!clinicAddress) {
            addressErrorSpan.text('Clinic address is required');
            $(this).find('.clinic-address').addClass('input-validation-error');
            rowValid = false;
            allClinicsValid = false;
        } else if (clinicAddress.length < 5) {
            addressErrorSpan.text('Clinic address must be at least 5 characters');
            $(this).find('.clinic-address').addClass('input-validation-error');
            rowValid = false;
            allClinicsValid = false;
        } else if (clinicAddress.length > 200) {
            addressErrorSpan.text('Clinic address cannot exceed 200 characters');
            $(this).find('.clinic-address').addClass('input-validation-error');
            rowValid = false;
            allClinicsValid = false;
        } else {
            addressErrorSpan.text('');
            $(this).find('.clinic-address').removeClass('input-validation-error');
        }

        if (rowValid && clinicName && clinicAddress) {
            hasValidClinic = true;
        }
    });

    if ($('.clinic-row').length === 0) {
        $('#clinicsGlobalError').text('Please add at least one clinic');
        isValid = false;
    } else if (!hasValidClinic) {
        $('#clinicsGlobalError').text('At least one complete clinic is required (both name and address)');
        isValid = false;
    } else {
        $('#clinicsGlobalError').text('');
    }

    if (!allClinicsValid) {
        isValid = false;
    }

    console.log("Doctor validation result:", isValid);
    return isValid;
};

// دالة لمسح أخطاء الدكتور
window.clearDoctorValidationErrors = function () {
    $('#specializationError').text('');
    $('#Specialization').removeClass('input-validation-error');
    $('.clinic-row').each(function () {
        $(this).find('.clinic-name-error').text('');
        $(this).find('.clinic-address-error').text('');
        $(this).find('.clinic-name').removeClass('input-validation-error');
        $(this).find('.clinic-address').removeClass('input-validation-error');
    });
    $('#clinicsGlobalError').text('');
};

// دالة إضافة عيادة جديدة
window.addClinic = function () {
    var container = document.getElementById('clinicsContainer');
    var newRow = document.createElement('div');
    newRow.className = 'row g-2 mb-2 clinic-row border-top pt-2 mt-2';
    newRow.innerHTML = `
        <div class="col-md-5">
            <input type="text" name="Clinics[${clinicCounter}].Name" class="form-control clinic-name" placeholder="Clinic Name" />
            <span class="clinic-name-error text-danger small"></span>
        </div>
        <div class="col-md-5">
            <input type="text" name="Clinics[${clinicCounter}].Address" class="form-control clinic-address" placeholder="Clinic Address" />
            <span class="clinic-address-error text-danger small"></span>
        </div>
        <div class="col-md-2">
            <button type="button" class="btn btn-sm btn-outline-danger remove-clinic w-100">Remove</button>
        </div>
    `;
    container.appendChild(newRow);
    clinicCounter++;

    // ✅ إضافة حدث التحقق فقط عند ترك الحقل (blur)
    $(newRow).find('.clinic-name, .clinic-address').on('blur', function () {
        if ($('#roleSelect').val() === 'Doctor') {
            window.validateDoctorFields();
        }
    });

    // إضافة حدث الحذف
    $(newRow).find('.remove-clinic').on('click', function () {
        if ($('.clinic-row').length > 1) {
            $(this).closest('.clinic-row').remove();
            if ($('#roleSelect').val() === 'Doctor') {
                window.validateDoctorFields();
            }
        } else {
            alert('You need at least one clinic');
        }
    });
};

// الحدث الرئيسي - كل شيء هنا
$(document).ready(function () {
    console.log("✅ Register.js loaded successfully");

    // ========== 1. إظهار وإخفاء حقول الدكتور ==========
    $('#roleSelect').on('change', function () {
        var doctorFields = $('#doctorFields');
        if (this.value === 'Doctor') {
            doctorFields.slideDown();
        } else {
            doctorFields.slideUp();
            window.clearDoctorValidationErrors();
        }
    }).trigger('change');

    // ========== 2. إضافة عيادة جديدة ==========
    $('#addClinicBtn').on('click', function () {
        window.addClinic();
    });

    // ========== 3. ✅ التحقق فقط عند ترك الحقل (blur) - وليس أثناء الكتابة ==========
    // الحقول الأساسية
    $('#Name').on('blur', function () { window.validateSingleField('Name'); });
    $('#Email').on('blur', function () { window.validateSingleField('Email'); });
    $('#Phone').on('blur', function () { window.validateSingleField('Phone'); });
    $('#Password').on('blur', function () { window.validateSingleField('Password'); });
    $('#ConfirmPassword').on('blur', function () { window.validateSingleField('ConfirmPassword'); });

    // حقول الدكتور - التحقق فقط عند ترك الحقل
    $(document).on('blur', '#Specialization', function () {
        if ($('#roleSelect').val() === 'Doctor') {
            window.validateDoctorFields();
        }
    });

    // ========== 4. منع إرسال الفورم والتحقق من صحة البيانات ==========
    $('#registerForm').on('submit', function (e) {
        console.log("Form submit triggered");

        // التحقق من جميع الحقول الأساسية
        const isBasicValid = window.validateAllBasicFields();

        // التحقق من صحة حقول الدكتور
        let isDoctorValid = true;
        if ($('#roleSelect').val() === 'Doctor') {
            isDoctorValid = window.validateDoctorFields();
        }

        console.log("Basic valid:", isBasicValid);
        console.log("Doctor valid:", isDoctorValid);

        if (!isBasicValid || !isDoctorValid) {
            e.preventDefault();
            e.stopPropagation();

            alert("❌ Please fix the errors in the form before submitting.\n\nCheck:\n- Name (3-50 characters)\n- Valid Email\n- Valid Egyptian phone number\n- Password (min 6 characters)\n- Password match");

            $('input.input-validation-error:first').focus();
            return false;
        }

        console.log("✅ Form validation passed, submitting...");
        return true;
    });

    console.log("✅ All event handlers registered successfully");
});