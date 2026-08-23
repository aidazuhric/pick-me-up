import 'package:json_annotation/json_annotation.dart';

part 'driver_location.g.dart';

@JsonSerializable()
class DriverLocation {
  int? driverId;
  String? driverName;
  String? companyName;
  String? carPlate;
  double? latitude;
  double? longitude;
  DateTime? updatedAt;
  bool? isOnShift;
  bool? isOnBreak;
  bool? isDispatchEligible;

  DriverLocation({
    this.driverId,
    this.driverName,
    this.companyName,
    this.carPlate,
    this.latitude,
    this.longitude,
    this.updatedAt,
    this.isOnShift,
    this.isOnBreak,
    this.isDispatchEligible,
  });

  factory DriverLocation.fromJson(Map<String, dynamic> json) =>
      _$DriverLocationFromJson(json);

  Map<String, dynamic> toJson() => _$DriverLocationToJson(this);
}
