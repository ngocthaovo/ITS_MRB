function CheckUpdateForm() {
    var form3 = $('#form_edit_account');
    var errorHandler3 = $('.errorHandler', form3);
    form3.validate({
        rules: {
            UserCodeID: {
                minlength: 4,
                required: true
            },
            UserName: {
                required: true
            },
            UserNameEN: {
                required: true
            },
            UserEmail: {
                required: true
            },
            UserPhone: {
                required: true
            },
            UserSex: {
                required: true
            },
            CostCenterCode: {
                required: true
            },
            UserPosstion: {
                required: true
            },
            UserStartDate: {
                required: true
            }
        },

        invalidHandler: function (event, validator) { //display error alert on form submit
            errorHandler3.show();
        }
    });
}
