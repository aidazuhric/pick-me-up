class OrderQuote {
  const OrderQuote({
    this.distanceKm,
    required this.fare,
    required this.currency,
    required this.isGeneralOrder,
    this.companyName,
  });

  final double? distanceKm;
  final double fare;
  final String currency;
  final bool isGeneralOrder;
  final String? companyName;

  factory OrderQuote.fromJson(Map<String, dynamic> json) => OrderQuote(
    distanceKm: (json['distanceKm'] as num?)?.toDouble(),
    fare: (json['fare'] as num?)?.toDouble() ?? 0,
    currency: json['currency'] as String? ?? 'BAM',
    isGeneralOrder: json['isGeneralOrder'] as bool? ?? false,
    companyName: json['companyName'] as String?,
  );
}
