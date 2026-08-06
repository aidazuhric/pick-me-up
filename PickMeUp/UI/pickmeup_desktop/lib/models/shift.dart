import 'package:json_annotation/json_annotation.dart';

part 'shift.g.dart';

@JsonSerializable()
class Shift {
  int? id;
  String? driverName;
  String? companyName;
  DateTime? startedAt;
  DateTime? endedAt;
  bool? isOngoing;
  bool? isOnBreak;
  int? durationMinutes;
  int? breakMinutes;
  int? workedMinutes;
  int? ordersHandled;

  Shift({
    this.id,
    this.driverName,
    this.companyName,
    this.startedAt,
    this.endedAt,
    this.isOngoing,
    this.isOnBreak,
    this.durationMinutes,
    this.breakMinutes,
    this.workedMinutes,
    this.ordersHandled,
  });

  factory Shift.fromJson(Map<String, dynamic> json) => _$ShiftFromJson(json);

  Map<String, dynamic> toJson() => _$ShiftToJson(this);
}
