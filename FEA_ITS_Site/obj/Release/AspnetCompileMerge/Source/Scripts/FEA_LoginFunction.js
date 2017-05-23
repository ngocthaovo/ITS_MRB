var FEAFunction = function () {
}

jQuery.validator.addMethod("EmployeeUID", function (phone_number, element) {
    phone_number = phone_number.replace(/\s+/g, "");
    return this.optional(element) ||
          phone_number.match(/^[0-9a-zA-Z]+$/);
}, "Invalid ID format");

function CheckChangePassForm()
{
    $("#msg_change_pass_success").css("display", "none");
    $("#msg_change_pass_error").css("display", "none");

    var form = $('#form-changepass');
    var errorHandler = $('.errorHandler', form);
    form.validate({
        rules: {
            change_pass_employeeid: {
                minlength: 4,
                required: true
            },
            change_pass_current_pass: {
                minlength: 6,
                required: true
            },
            change_pass_new_pass: {
                minlength: 6,
                required: true
            },
            change_pass_confirm_pass: {
                equalTo: "#change_pass_new_pass",
                required: true
            }

        },
        invalidHandler: function (event, validator) { //display error alert on form submit
            errorHandler.show();
        }
    });
}
function CheckLoginForm() {
    var form = $('#form-login');
    var errorHandler = $('.errorHandler', form);
    form.validate({
        rules: {
            username: {
                minlength: 4,
                required: true
            },
            password: {
                minlength: 6,
                required: true
            }
        },
        invalidHandler: function (event, validator) { //display error alert on form submit
            errorHandler.show();
        }
    });
}
function CheckRegistrationForm() {
    var form3 = $('#form-register');
    var errorHandler3 = $('.errorHandler', form3);
    form3.validate({
        rules: {
            res_employee_code: {
                minlength: 4,
                EmployeeUID: true,
                required: true
            },
            res_employee_name: {
                required: true
            },
            res_employee_english_name: {
                required: true
            },
            gender: {
                required: true
            },
            res_email: {
                required: true
            },
            res_phone: {
                minlength: 4,
                required: true
            },
            cboCodeCenter: {
                required: true
            },
            cboUserPosition: {
                required: true
            }
        },

        invalidHandler: function (event, validator) { //display error alert on form submit
            errorHandler3.show();
        }
    });
}

//
function onLoginByAjax(e) {
    if (e.ErrorCode == 0) {// Login false
        $("#lblLogin_Error").css("display",'block')
    }
    else if (e.ErrorCode == -1) {//password Expired
        alert(e.Message);
        $("#change_pass_employeeid").val($("#log_usercode").val());
        $(".forgot").click();
    }
    else if (e.ErrorCode == 1) {//Success
        location.reload();
    }
}
function onRegistrationByAjax(e) {
    if (e.ErrorCode == 0) {// success
        alert(e.Message);
        document.location.reload();
    }
    else { alert(e.Message); }
}
function onChangePassByAjax(e)
{
    if (e.ErrorCode == 1)
    {
        $("#msg_change_pass_success").css("display", "block");
        $("#msg_change_pass_error").css("display", "none");

        var mess = $("#msg_change_pass_success_mes").text();
        alert(mess);
        $("#log_userpass").val('');
        $(".go-back").click();
    }
    else
    {
        $("#msg_change_pass_success").css("display", "none");
        $("#msg_change_pass_error").css("display", "block");
    }
}