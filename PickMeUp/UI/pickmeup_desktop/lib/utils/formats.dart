class Formats {
  Formats._();

  static String _two(int value) => value.toString().padLeft(2, '0');

  static String day(DateTime? value) {
    if (value == null) return '';
    final local = value.toLocal();
    return '${local.year}-${_two(local.month)}-${_two(local.day)}';
  }

  static String moment(DateTime? value) {
    if (value == null) return '';
    final local = value.toLocal();
    return '${day(value)} ${_two(local.hour)}:${_two(local.minute)}';
  }

  static String money(double? amount, [String currency = 'BAM']) =>
      amount == null ? '' : '${amount.toStringAsFixed(2)} $currency';

  static String minutes(int? total) {
    if (total == null) return '';
    if (total < 60) return '$total min';
    final hours = total ~/ 60;
    final rest = total % 60;
    return rest == 0 ? '${hours}h' : '${hours}h ${rest}min';
  }

  static String rating(double? average, int? count) {
    if (average == null || (count ?? 0) == 0) return 'no reviews';
    final reviews = count == 1 ? '1 review' : '$count reviews';
    return '${average.toStringAsFixed(2)} · $reviews';
  }

  static String statusLabel(String? status) =>
      status == 'InProgress' ? 'In progress'
          : status == 'PartiallyRefunded' ? 'Partially refunded'
          : status ?? '';
}
