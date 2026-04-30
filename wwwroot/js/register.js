$(document).ready(function () {
    let clinicCounter = 1;

    // إظهار وإخفاء حقول الدكتور
    $('#roleSelect').on('change', function () {
        var doctorFields = $('#doctorFields');
        if (this.value === 'Doctor') {
            doctorFields.slideDown();
        } else {
            doctorFields.slideUp();
            clearDoctorValidationErrors();
        }
    });

    // دالة لمسح أخطاء الدكتور
    window.clearDoctorValidationErrors = function () {
        $('#specializationError').text('');
        $('#Specialization').removeClass('input-validation-error');
        $('.clinic-name').each(function () {
            $(this).removeClass('input-validation-error');
            $(this).siblings('.clinic-name-error').text('');
        });
        $('.clinic-address').each(function () {
            $(this).removeClass('input-validation-error');
            $(this).siblings('.clinic-address-error').text('');
        });
        $('#clinicsGlobalError').text('');
    };

    // منع إرسال الفورم والتحقق من صحة البيانات
    $('form').on('submit', function (e) {
        const isBasicValid = $(this).valid();
        const isDoctorValid = window.validateDoctorFields();

        if (!isBasicValid || !isDoctorValid) {
            e.preventDefault();
            return false;
        }
        return true;
    });

    // Validation فوري للحقول الأساسية
    $('form input').on('blur', function () {
        $(this).valid();
    });

    // إعداد الـ validation الأساسي
    $('form').validate({
        rules: {
            Name: {
                required: true,
                minlength: 3,
                maxlength: 50
            },
            Email: {
                required: true,
                email: true
            },
            phone: {
                required: true,
                pattern: /^01[0125][0-9]{8}$/
            },
            Password: {
                required: true,
                minlength: 6
            },
            ConfirmPass: {
                required: true,
                equalTo: "#Password"
            }
        },
        messages: {
            Name: {
                required: "Full name is required",
                minlength: "Name must be at least 3 characters",
                maxlength: "Name cannot exceed 50 characters"
            },
            Email: {
                required: "Email is required",
                email: "Please enter a valid email address"
            },
            phone: {
                required: "Phone number is required",
                pattern: "Please enter a valid Egyptian phone number"
            },
            Password: {
                required: "Password is required",
                minlength: "Password must be at least 6 characters"
            },
            ConfirmPass: {
                required: "Please confirm your password",
                equalTo: "Passwords do not match"
            }
        },
        errorElement: "span",
        errorClass: "text-danger small",
        errorPlacement: function (error, element) {
            var errorSpan = element.siblings('.text-danger');
            if (errorSpan.length) {
                var modelError = errorSpan.text();
                if (modelError) {
                    errorSpan.text(modelError);
                }
            }
        },
        highlight: function (element) {
            $(element).addClass("input-validation-error");
        },
        unhighlight: function (element) {
            $(element).removeClass("input-validation-error");
        }
    });

    // إضافة عيادة جديدة
    $('#addClinicBtn').on('click', function () {
        addClinic();
    });
});

// دالة إضافة عيادة جديدة
function addClinic() {
    var container = document.getElementById('clinicsContainer');
    var newRow = document.createElement('div');
    newRow.className = 'row g-2 mb-2 clinic-row';
    newRow.innerHTML = `
        <div class="col-md-6">
            <input type="text" name="ClinicNames" class="form-control clinic-name" placeholder="Clinic Name" />
            <span class="clinic-name-error text-danger small"></span>
        </div>
        <div class="col-md-6">
            <input type="text" name="ClinicAddresses" class="form-control clinic-address" placeholder="Clinic Address" />
            <span class="clinic-address-error text-danger small"></span>
        </div>
        <div class="col-md-12 text-end mt-1">
            <button type="button" class="btn btn-sm btn-danger remove-clinic">Remove</button>
        </div>
    `;
    container.appendChild(newRow);

    // إضافة أحداث الـ validation للحقول الجديدة
    $(newRow).find('.clinic-name, .clinic-address').on('blur input', function () {
        if ($('#roleSelect').val() === 'Doctor') {
            window.validateDoctorFields();
        }
    });

    // إضافة زر الحذف
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
}