import 'package:flutter/material.dart';

import '../models/city.dart';
import '../models/taxi_company.dart';
import '../providers/city_provider.dart';
import '../providers/taxi_company_provider.dart';
import '../utils/api_error_handler.dart';
import '../utils/validators.dart';
import '../widgets/app_messages.dart';
import '../widgets/db_dropdown.dart';
import '../core/theme/app_spacing.dart';
import '../widgets/labeled_form.dart';

class TaxiCompanyForm extends StatefulWidget {
  const TaxiCompanyForm({super.key, required this.cityProvider, required this.companyProvider});

  final CityProvider cityProvider;
  final TaxiCompanyProvider companyProvider;

  static Future<bool> show(
    BuildContext context, {
    required CityProvider cityProvider,
    required TaxiCompanyProvider companyProvider,
  }) async {
    final created = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: SizedBox(
            width: 760,
            child: TaxiCompanyForm(
              cityProvider: cityProvider,
              companyProvider: companyProvider,
            ),
          ),
        ),
      ),
    );

    return created ?? false;
  }

  @override
  State<TaxiCompanyForm> createState() => _TaxiCompanyFormState();
}

class _TaxiCompanyFormState extends State<TaxiCompanyForm> {
  final _formKey = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _address = TextEditingController();
  final _owner = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _startingPrice = TextEditingController();
  final _pricePerKm = TextEditingController();
  final _loginUsername = TextEditingController();

  int? _cityId;
  bool _isActive = true;
  bool _isSaving = false;

  @override
  void dispose() {
    for (final c in [
      _name,
      _address,
      _owner,
      _email,
      _phone,
      _startingPrice,
      _pricePerKm,
      _loginUsername,
    ]) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);

    try {
      final created = await widget.companyProvider.insert({
        'name': _name.text.trim(),
        'hqAddress': _address.text.trim(),
        'cityId': _cityId,
        'contactEmail': _email.text.trim(),
        'contactPhone': _phone.text.trim(),
        'ownerName': _owner.text.trim(),
        'startingPrice': double.parse(_startingPrice.text.trim().replaceAll(',', '.')),
        'pricePerKm': double.parse(_pricePerKm.text.trim().replaceAll(',', '.')),
        'isActive': _isActive,
        'loginUsername': _loginUsername.text.trim(),
      });

      if (!mounted) return;

      Navigator.of(context).pop(true);
      _announceCreated(created);
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSaving = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  void _announceCreated(TaxiCompany created) {
    final password = created.initialLoginPassword;

    AppMessages.success(
      context,
      password == null
          ? '${created.name} was added in ${created.cityName}.'
          : '${created.name} was added in ${created.cityName}. '
              'Login ${created.initialLoginUsername} with temporary password $password.',
    );
  }

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 640,
      child: LabeledForm(
        title: 'New taxi company',
        formKey: _formKey,
        isSaving: _isSaving,
        onBack: _isSaving ? null : () => Navigator.of(context).pop(false),
        onSave: _save,
        fields: [
          LabeledField(
            label: 'Company name',
            isRequired: true,
            child: AppTextField(
              controller: _name,
              validator: (v) =>
                  Validators.required(v, 'Company name') ?? Validators.maxLength(v, 150, 'Company name'),
            ),
          ),
          LabeledField(
            label: 'Headquarters address',
            isRequired: true,
            child: AppTextField(
              controller: _address,
              validator: (v) => Validators.required(v, 'Headquarters address'),
            ),
          ),
          LabeledField(
            label: 'City',
            isRequired: true,
            child: DbDropdown<City>(
              loadOptions: () async {
                final result = await widget.cityProvider.get(
                  filter: {'PageSize': 100, 'IsActive': true},
                );
                return result.items ?? const [];
              },
              labelOf: (c) => c.name ?? '',
              idOf: (c) => c.id ?? 0,
              value: _cityId,
              onChanged: (v) => setState(() => _cityId = v),
              emptyMessage: 'No cities exist yet. Add a city before creating a company.',
              validator: (v) => Validators.chosen(v, 'a city'),
            ),
          ),
          LabeledField(
            label: 'Owner',
            isRequired: true,
            child: AppTextField(
              controller: _owner,
              validator: (v) => Validators.required(v, 'Owner name'),
            ),
          ),
          LabeledField(
            label: 'Contact email',
            isRequired: true,
            child: AppTextField(controller: _email, validator: Validators.email),
          ),
          LabeledField(
            label: 'Contact phone',
            isRequired: true,
            child: AppTextField(controller: _phone, validator: Validators.phone),
          ),
          LabeledField(
            label: 'Starting price',
            isRequired: true,
            child: AppTextField(
              controller: _startingPrice,
              keyboardType: TextInputType.number,
              validator: (v) => Validators.money(v, 'Starting price'),
            ),
          ),
          LabeledField(
            label: 'Price per kilometre',
            isRequired: true,
            child: AppTextField(
              controller: _pricePerKm,
              keyboardType: TextInputType.number,
              validator: (v) => Validators.money(v, 'Price per kilometre'),
            ),
          ),
          LabeledField(
            label: 'Company login',
            isRequired: true,
            child: AppTextField(
              controller: _loginUsername,
              validator: (v) => Validators.required(v, 'Login username'),
            ),
          ),
          LabeledField(
            label: 'Active',
            child: AppToggleField(
              value: _isActive,
              onChanged: (v) => setState(() => _isActive = v),
              description: 'Passengers can order from this company',
            ),
          ),
        ],
      ),
    );
  }
}
