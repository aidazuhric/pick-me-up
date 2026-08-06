import 'package:json_annotation/json_annotation.dart';

part 'car.g.dart';

@JsonSerializable()
class Car {
  int? id;
  String? make;
  String? model;
  String? plate;
  int? year;
  int? seats;
  String? categoryName;
  String? companyName;
  String? photoImageUrl;
  bool? isActive;

  Car({
    this.id,
    this.make,
    this.model,
    this.plate,
    this.year,
    this.seats,
    this.categoryName,
    this.companyName,
    this.photoImageUrl,
    this.isActive,
  });

  String get description => [make, model].where((p) => p != null).join(' ');

  factory Car.fromJson(Map<String, dynamic> json) => _$CarFromJson(json);

  Map<String, dynamic> toJson() => _$CarToJson(this);
}
