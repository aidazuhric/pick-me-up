import 'package:json_annotation/json_annotation.dart';

part 'payable_order.g.dart';

@JsonSerializable()
class PayableOrder {
  int? orderId;
  String? companyName;
  String? driverName;
  String? pickupAddress;
  String? destinationAddress;
  double? distanceKm;
  double? fare;
  DateTime? completedAt;

  PayableOrder({
    this.orderId,
    this.companyName,
    this.driverName,
    this.pickupAddress,
    this.destinationAddress,
    this.distanceKm,
    this.fare,
    this.completedAt,
  });

  factory PayableOrder.fromJson(Map<String, dynamic> json) =>
      _$PayableOrderFromJson(json);

  Map<String, dynamic> toJson() => _$PayableOrderToJson(this);
}
