class Validators {
  Validators._();

  static final RegExp _email = RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$');
  static final RegExp _phone = RegExp(r'^\+?[0-9\s\-]{6,20}$');

  static String? required(String? value, String field) {
    if (value == null || value.trim().isEmpty) {
      return '$field is required.';
    }
    return null;
  }

  static String? minLength(String? value, int min, String field) {
    if (value != null && value.trim().length < min) {
      return '$field must be at least $min characters.';
    }
    return null;
  }

  static String? maxLength(String? value, int max, String field) {
    if (value != null && value.length > max) {
      return '$field must be at most $max characters.';
    }
    return null;
  }

  static String? email(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Email is required.';
    }
    if (!_email.hasMatch(value.trim())) {
      return 'Enter a valid email address, for example name@company.com.';
    }
    return null;
  }

  static String? phone(String? value) {
    if (value == null || value.trim().isEmpty) {
      return 'Phone number is required.';
    }
    if (!_phone.hasMatch(value.trim())) {
      return 'Enter a valid phone number, digits only with an optional leading +, for example +38736111222.';
    }
    return null;
  }

  static String? money(String? value, String field) {
    if (value == null || value.trim().isEmpty) {
      return '$field is required.';
    }
    final parsed = double.tryParse(value.trim().replaceAll(',', '.'));
    if (parsed == null) {
      return 'Enter $field as a number, for example 2.50.';
    }
    if (parsed < 0) {
      return '$field cannot be negative.';
    }
    return null;
  }

  static String? chosen(int? value, String field) {
    if (value == null) {
      return 'Select $field from the list.';
    }
    return null;
  }
}
