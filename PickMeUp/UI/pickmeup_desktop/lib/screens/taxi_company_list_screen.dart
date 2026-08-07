import 'package:flutter/material.dart';

import '../models/search_result.dart';
import '../models/taxi_company.dart';
import '../providers/city_provider.dart';
import '../providers/taxi_company_provider.dart';
import '../utils/api_error_handler.dart';
import '../widgets/app_messages.dart';
import '../widgets/confirm_dialog.dart';
import '../widgets/crud_data_table.dart';
import 'taxi_company_form.dart';

class TaxiCompanyListScreen extends StatefulWidget {
  const TaxiCompanyListScreen({super.key});

  @override
  State<TaxiCompanyListScreen> createState() => _TaxiCompanyListScreenState();
}

class _TaxiCompanyListScreenState extends State<TaxiCompanyListScreen> {
  final _companyProvider = TaxiCompanyProvider();
  final _cityProvider = CityProvider();

  Future<SearchResult<TaxiCompany>> _load(
    int page,
    int pageSize,
    String? search,
    String? sortBy,
  ) {
    return _companyProvider.get(filter: {
      'Page': page,
      'PageSize': pageSize,
      'IncludeTotalCount': true,
      'SortBy': ?sortBy,
      'Name': ?search,
    });
  }

  Future<bool> _create() {
    return TaxiCompanyForm.show(
      context,
      cityProvider: _cityProvider,
      companyProvider: _companyProvider,
    );
  }

  Future<bool> _delete(TaxiCompany company) async {
    final confirmed = await ConfirmDialog.show(
      context,
      title: 'Remove ${company.name}?',
      message:
          'The company stops appearing for passengers. Its rides and payments are kept for the record.',
      confirmLabel: 'DELETE',
      isDestructive: true,
    );

    if (!confirmed) {
      return false;
    }

    try {
      await _companyProvider.remove(company.id!);
      if (!mounted) return false;
      AppMessages.success(context, '${company.name} was removed.');
      return true;
    } catch (e) {
      if (!mounted) return false;
      ApiErrorHandler.surface(context, e);
      return false;
    }
  }

  String _money(double? value) => value == null ? '' : value.toStringAsFixed(2);

  @override
  Widget build(BuildContext context) {
    return CrudDataTable<TaxiCompany>(
      title: 'Taxi companies',
      searchHint: 'Search by company name',
      loadPage: _load,
      onCreate: _create,
      onDelete: _delete,
      deleteGuard: (company) => company.isActive == true
          ? RowGuard.allowed
          : const RowGuard(
              isAllowed: false,
              reason: 'This company is already inactive.',
            ),
      imageUrlOf: (company) => company.logoImageUrl,
      imageFit: BoxFit.contain,
      columns: [
        CrudColumn(label: 'Company name', value: (c) => c.name ?? '', sortKey: 'Name'),
        CrudColumn(label: 'City', value: (c) => c.cityName ?? ''),
        CrudColumn(label: 'Address', value: (c) => c.hqAddress ?? ''),
        CrudColumn(label: 'Owner', value: (c) => c.ownerName ?? '', sortKey: 'OwnerName'),
        CrudColumn(label: 'Contact', value: (c) => c.contactPhone ?? ''),
        CrudColumn(
          label: 'Starting price',
          value: (c) => _money(c.startingPrice),
          sortKey: 'StartingPrice',
        ),
        CrudColumn(
          label: 'Price / km',
          value: (c) => _money(c.pricePerKm),
          sortKey: 'PricePerKm',
        ),
      ],
    );
  }
}
