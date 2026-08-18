import '../models/taxi_company.dart';
import 'base_provider.dart';

class TaxiCompanyProvider extends BaseProvider<TaxiCompany> {
  TaxiCompanyProvider() : super('TaxiCompanies');

  @override
  TaxiCompany fromJson(dynamic data) =>
      TaxiCompany.fromJson(data as Map<String, dynamic>);
}
