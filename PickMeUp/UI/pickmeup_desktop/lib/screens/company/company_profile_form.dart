import 'package:flutter/material.dart';

import '../../core/theme/app_spacing.dart';
import '../../models/city.dart';
import '../../models/taxi_company.dart';
import '../../providers/city_provider.dart';
import '../../providers/taxi_company_provider.dart';
import '../../utils/api_error_handler.dart';
import '../../utils/validators.dart';
import '../../widgets/app_messages.dart';
import '../../widgets/db_dropdown.dart';
import '../../widgets/labeled_form.dart';

class CompanyProfileForm extends StatefulWidget {
  const CompanyProfileForm({
    super.key,
    required this.company,
    required this.provider,
    required this.cityProvider,
  });

  final TaxiCompany company;
  final TaxiCompanyProvider provider;
  final CityProvider cityProvider;

  static Future<bool> show(
    BuildContext context, {
    required TaxiCompany company,
    required TaxiCompanyProvider provider,
    required CityProvider cityProvider,
  }) async {
    final saved = await showDialog<bool>(
      context: context,
      barrierDismissible: false,
      builder: (_) => Dialog(
        insetPadding: const EdgeInsets.symmetric(horizontal: 80, vertical: 40),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: SizedBox(
            width: 760,
            height: 660,
            child: CompanyProfileForm(
              company: company,
              provider: provider,
              cityProvider: cityProvider,
            ),
          ),
        ),
      ),
    );

    return saved ?? false;
  }

  @override
  State<CompanyProfileForm> createState() => _CompanyProfileFormState();
}

class _CompanyProfileFormState extends State<CompanyProfileForm> {
  final _formKey = GlobalKey<FormState>();
  final _name = TextEditingController();
  final _address = TextEditingController();
  final _owner = TextEditingController();
  final _email = TextEditingController();
  final _phone = TextEditingController();
  final _startingPrice = TextEditingController();
  final _pricePerKm = TextEditingController();

  int? _cityId;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();

    final company = widget.company;
    _name.text = company.name ?? '';
    _address.text = company.hqAddress ?? '';
    _owner.text = company.ownerName ?? '';
    _email.text = company.contactEmail ?? '';
    _phone.text = company.contactPhone ?? '';
    _startingPrice.text = company.startingPrice?.toStringAsFixed(2) ?? '';
    _pricePerKm.text = company.pricePerKm?.toStringAsFixed(2) ?? '';
  }

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
    ]) {
      c.dispose();
    }
    super.dispose();
  }

  double _money(TextEditingController controller) =>
      double.parse(controller.text.trim().replaceAll(',', '.'));

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);

    try {
      final saved = await widget.provider.updateMine({
        'name': _name.text.trim(),
        'hqAddress': _address.text.trim(),
        'cityId': _cityId,
        'contactEmail': _email.text.trim(),
        'contactPhone': _phone.text.trim(),
        'ownerName': _owner.text.trim(),
        'startingPrice': _money(_startingPrice),
        'pricePerKm': _money(_pricePerKm),
      });

      if (!mounted) return;

      AppMessages.success(context, '${saved.name} was saved.');
      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      setState(() => _isSaving = false);
      ApiErrorHandler.surface(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return LabeledForm(
      title: 'Edit company profile',
      subtitle: 'How your company appears to passengers, and the prices your fares are built from.',
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
                Validators.required(v, 'Company name') ??
                Validators.maxLength(v, 150, 'Company name'),
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
          hint: 'Loaded from the database',
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
            hintText: widget.company.cityName ?? 'Choose one',
            onChanged: (value) => setState(() => _cityId = value),
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
          hint: 'BAM, charged on every ride',
          child: AppTextField(
            controller: _startingPrice,
            keyboardType: TextInputType.number,
            validator: (v) => Validators.money(v, 'Starting price'),
          ),
        ),
        LabeledField(
          label: 'Price per kilometre',
          isRequired: true,
          hint: 'BAM per kilometre driven',
          child: AppTextField(
            controller: _pricePerKm,
            keyboardType: TextInputType.number,
            validator: (v) => Validators.money(v, 'Price per kilometre'),
          ),
        ),
      ],
    );
  }
}
