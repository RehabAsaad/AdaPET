// دالة التحقق من صحة حقول الدكتور
window.validateDoctorFields = function () {
    let isValid = true;

    // لو المستخدم مش دكتور، نعتبر الـ validation ناجح
    if ($('#roleSelect').val() !== 'Doctor') {
        return true;
    }

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

    // 2. التحقق من العيادات (على الأقل عيادة واحدة مكتملة)
    let hasValidClinic = false;
    let allClinicsValid = true;

    $('.clinic-row').each(function (index) {
        const clinicName = $(this).find('.clinic-name').val().trim();
        const clinicAddress = $(this).find('.clinic-address').val().trim();
        const nameErrorSpan = $(this).find('.clinic-name-error');
        const addressErrorSpan = $(this).find('.clinic-address-error');

        let rowValid = true;

        // التحقق من اسم العيادة
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

        // التحقق من عنوان العيادة
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

    // التأكد من وجود عيادة واحدة على الأقل مكتملة
    if (!hasValidClinic && $('.clinic-row').length > 0) {
        $('#clinicsGlobalError').text('At least one complete clinic is required (both name and address)');
        isValid = false;
    } else {
        $('#clinicsGlobalError').text('');
    }

    if (!allClinicsValid) {
        isValid = false;
    }

    return isValid;
};

// إضافة أحداث الـ validation الفورية
$(document).ready(function () {
    // Validation فوري للدكتور عند الخروج من الحقول
    $(document).on('blur', '#Specialization', function () {
        if ($('#roleSelect').val() === 'Doctor') {
            window.validateDoctorFields();
        }
    });

    $(document).on('blur', '.clinic-name, .clinic-address', function () {
        if ($('#roleSelect').val() === 'Doctor') {
            window.validateDoctorFields();
        }
    });

    // Validation فوري أثناء الكتابة
    $(document).on('input', '#Specialization, .clinic-name, .clinic-address', function () {
        if ($('#roleSelect').val() === 'Doctor') {
            window.validateDoctorFields();
        }
    });
});