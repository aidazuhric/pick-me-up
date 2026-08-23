import 'package:json_annotation/json_annotation.dart';

part 'shift.g.dart';

@JsonSerializable()
class ShiftBreak {
  DateTime? startedAt;
  DateTime? endedAt;
  bool? isOngoing;
  int? durationMinutes;

  ShiftBreak({
    this.startedAt,
    this.endedAt,
    this.isOngoing,
    this.durationMinutes,
  });

  factory ShiftBreak.fromJson(Map<String, dynamic> json) =>
      _$ShiftBreakFromJson(json);

  Map<String, dynamic> toJson() => _$ShiftBreakToJson(this);
}

@JsonSerializable()
class Shift {
  int? id;
  String? driverName;
  String? companyName;
  DateTime? startedAt;
  DateTime? endedAt;
  bool? isOngoing;
  bool? isOnBreak;
  bool? isDispatchEligible;
  int? durationMinutes;
  int? breakMinutes;
  int? workedMinutes;
  int? ordersHandled;
  List<ShiftBreak>? breaks;

  Shift({
    this.id,
    this.driverName,
    this.companyName,
    this.startedAt,
    this.endedAt,
    this.isOngoing,
    this.isOnBreak,
    this.isDispatchEligible,
    this.durationMinutes,
    this.breakMinutes,
    this.workedMinutes,
    this.ordersHandled,
    this.breaks,
  });

  factory Shift.fromJson(Map<String, dynamic> json) => _$ShiftFromJson(json);

  Map<String, dynamic> toJson() => _$ShiftToJson(this);
}
