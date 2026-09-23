import 'package:json_annotation/json_annotation.dart';

part 'driver.g.dart';

@JsonSerializable()
class Driver {
  int? id;
  String? fullName;
  String? email;
  String? phoneNumber;
  String? companyName;
  DateTime? hireDate;
  DateTime? resignedAt;
  bool? isActive;
  String? initialLoginUsername;

  Driver({
    this.id,
    this.fullName,
    this.email,
    this.phoneNumber,
    this.companyName,
    this.hireDate,
    this.resignedAt,
    this.isActive,
    this.initialLoginUsername,
  });

  bool get hasResigned => resignedAt != null;

  factory Driver.fromJson(Map<String, dynamic> json) => _$DriverFromJson(json);

  Map<String, dynamic> toJson() => _$DriverToJson(this);
}
