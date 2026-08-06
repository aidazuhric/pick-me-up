import 'package:json_annotation/json_annotation.dart';

part 'order.g.dart';

@JsonSerializable()
class OrderStatusHistoryEntry {
  String? fromStatus;
  String? toStatus;
  String? changedBy;
  DateTime? changedAt;
  String? reason;

  OrderStatusHistoryEntry({
    this.fromStatus,
    this.toStatus,
    this.changedBy,
    this.changedAt,
    this.reason,
  });

  factory OrderStatusHistoryEntry.fromJson(Map<String, dynamic> json) =>
      _$OrderStatusHistoryEntryFromJson(json);

  Map<String, dynamic> toJson() => _$OrderStatusHistoryEntryToJson(this);
}

@JsonSerializable()
class Order {
  int? id;
  String? passengerName;
  String? companyName;
  String? driverName;
  String? carPlate;
  double? pickupLat;
  double? pickupLng;
  double? destinationLat;
  double? destinationLng;
  String? pickupAddress;
  String? destinationAddress;
  double? distanceKm;
  double? fare;
  String? status;
  bool? isGeneralOrder;
  DateTime? createdAt;
  DateTime? completedAt;
  DateTime? cancelledAt;
  String? cancellationReason;
  List<OrderStatusHistoryEntry>? statusHistory;

  Order({
    this.id,
    this.passengerName,
    this.companyName,
    this.driverName,
    this.carPlate,
    this.pickupLat,
    this.pickupLng,
    this.destinationLat,
    this.destinationLng,
    this.pickupAddress,
    this.destinationAddress,
    this.distanceKm,
    this.fare,
    this.status,
    this.isGeneralOrder,
    this.createdAt,
    this.completedAt,
    this.cancelledAt,
    this.cancellationReason,
    this.statusHistory,
  });

  String get route =>
      '${pickupAddress ?? ''} → ${destinationAddress ?? 'no destination'}';

  factory Order.fromJson(Map<String, dynamic> json) => _$OrderFromJson(json);

  Map<String, dynamic> toJson() => _$OrderToJson(this);
}
