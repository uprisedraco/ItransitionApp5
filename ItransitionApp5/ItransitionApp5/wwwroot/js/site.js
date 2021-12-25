function check_uncheck_checkbox(isChecked) {
	if (isChecked) {
		$('input[name="ids"]').each(function () {
			this.checked = true;
		});
	} else {
		$('input[name="ids"]').each(function () {
			this.checked = false;
		});
	}
}