class ApiDates {
  ApiDates._();

  static String dayToUtc(DateTime value) {
    final local = value.toLocal();
    return DateTime.utc(local.year, local.month, local.day).toIso8601String();
  }

  static String? dayToUtcOrNull(DateTime? value) =>
      value == null ? null : dayToUtc(value);
}
