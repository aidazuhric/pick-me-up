import 'package:json_annotation/json_annotation.dart';

part 'taxi_company.g.dart';

@JsonSerializable()
class TaxiCompany {
  int? id;
  String? name;
  String? hqAddress;
  String? cityName;
  String? contactPhone;
  double? startingPrice;
  double? pricePerKm;
  String? logoImageUrl;
  bool? isActive;

  TaxiCompany({
    this.id,
    this.name,
    this.hqAddress,
    this.cityName,
    this.contactPhone,
    this.startingPrice,
    this.pricePerKm,
    this.logoImageUrl,
    this.isActive,
  });

  factory TaxiCompany.fromJson(Map<String, dynamic> json) =>
      _$TaxiCompanyFromJson(json);

  Map<String, dynamic> toJson() => _$TaxiCompanyToJson(this);
}
