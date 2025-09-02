class Validators {
  static String? required(String? value) =>
      (value == null || value.trim().isEmpty) ? 'Required' : null;

  static String? email(String? value) {
    if (value == null || value.isEmpty) return 'Required';
    final r = RegExp(r'^[^@\s]+@[^@\s]+\.[^@\s]+$');
    if (!r.hasMatch(value)) return 'Invalid email';
    return null;
  }

  static String? minLen(String? value, int min) {
    if (value == null || value.length < min) return 'Min $min chars';
    return null;
  }
}
