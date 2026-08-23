class Formats {
  Formats._();

  static String money(double? amount, [String currency = 'BAM']) =>
      amount == null ? '' : '${amount.toStringAsFixed(2)} $currency';

  static String distance(double? km) =>
      km == null ? '' : '${km.toStringAsFixed(2)} km';

  static String _two(int value) => value.toString().padLeft(2, '0');

  static String day(DateTime? value) {
    if (value == null) return '';
    final local = value.toLocal();
    return '${local.day}.${local.month}.${local.year}.';
  }

  static String moment(DateTime? value) {
    if (value == null) return '';
    final local = value.toLocal();
    return '${day(value)} ${_two(local.hour)}:${_two(local.minute)}';
  }

  static String minutes(int? total) {
    if (total == null) return '';
    if (total < 60) return '$total min';
    final hours = total ~/ 60;
    final rest = total % 60;
    return rest == 0 ? '${hours}h' : '${hours}h ${rest}min';
  }

  static String statusLabel(String? status) => switch (status) {
    'InProgress' => 'In progress',
    'PartiallyRefunded' => 'Partially refunded',
    _ => status ?? '',
  };
}
