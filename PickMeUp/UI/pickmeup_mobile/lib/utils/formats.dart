class Formats {
  Formats._();

  static String money(double? amount, [String currency = 'BAM']) =>
      amount == null ? '' : '${amount.toStringAsFixed(2)} $currency';

  static String distance(double? km) =>
      km == null ? '' : '${km.toStringAsFixed(2)} km';
}
