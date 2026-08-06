import 'package:json_annotation/json_annotation.dart';

part 'car_assignment.g.dart';

@JsonSerializable()
class CarAssignment {
  int? id;
  String? carPlate;
  String? carMake;
  String? carModel;
  String? categoryName;
  String? driverName;
  String? companyName;
  DateTime? assignedFrom;
  DateTime? assignedTo;
  bool? isOngoing;
  bool? isActive;

  CarAssignment({
    this.id,
    this.carPlate,
    this.carMake,
    this.carModel,
    this.categoryName,
    this.driverName,
    this.companyName,
    this.assignedFrom,
    this.assignedTo,
    this.isOngoing,
    this.isActive,
  });

  String get carDescription =>
      [carMake, carModel].where((p) => p != null).join(' ');

  factory CarAssignment.fromJson(Map<String, dynamic> json) =>
      _$CarAssignmentFromJson(json);

  Map<String, dynamic> toJson() => _$CarAssignmentToJson(this);
}
