import 'package:json_annotation/json_annotation.dart';

part 'taxi_company.g.dart';

@JsonSerializable()
class TaxiCompany {
  int? id;
  String? name;
  String? hqAddress;
  String? cityName;
  String? contactEmail;
  String? contactPhone;
  String? ownerName;
  double? startingPrice;
  double? pricePerKm;
  String? logoImageUrl;
  bool? isActive;
  DateTime? createdAt;
  DateTime? updatedAt;
  String? initialLoginUsername;
  String? initialLoginPassword;

  TaxiCompany({
    this.id,
    this.name,
    this.hqAddress,
    this.cityName,
    this.contactEmail,
    this.contactPhone,
    this.ownerName,
    this.startingPrice,
    this.pricePerKm,
    this.logoImageUrl,
    this.isActive,
    this.createdAt,
    this.updatedAt,
    this.initialLoginUsername,
    this.initialLoginPassword,
  });

  factory TaxiCompany.fromJson(Map<String, dynamic> json) => _$TaxiCompanyFromJson(json);

  Map<String, dynamic> toJson() => _$TaxiCompanyToJson(this);
}
