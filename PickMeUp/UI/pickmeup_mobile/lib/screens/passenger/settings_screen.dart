import 'dart:io';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';

import '../../core/theme/app_colors.dart';
import '../../core/constants/roles.dart';
import '../../core/theme/app_spacing.dart';
import '../../models/user.dart';
import '../../providers/auth_provider.dart';
import '../../providers/driver_provider.dart';
import '../../providers/notification_store.dart';
import '../../providers/user_provider.dart';
import '../../utils/api_client_exception.dart';
import '../../utils/validators.dart';
import '../../widgets/api_image.dart';
import '../../widgets/app_messages.dart';
import 'change_password_screen.dart';

class SettingsScreen extends StatefulWidget {
  const SettingsScreen({super.key});

  @override
  State<SettingsScreen> createState() => _SettingsScreenState();
}

class _SettingsScreenState extends State<SettingsScreen> {
  final _formKey = GlobalKey<FormState>();
  final _firstNameController = TextEditingController();
  final _lastNameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _users = UserProvider();
  final _drivers = DriverProvider();
  final _picker = ImagePicker();

  User? _me;
  bool _isLoading = true;
  bool _isSaving = false;
  bool _isEditing = false;
  bool _twoFactorEnabled = false;
  bool _isTogglingTwoFactor = false;
  bool _isResigning = false;
  bool _isDeleting = false;
  bool _isUploadingPhoto = false;
  int _photoVersion = 0;

  static const int _maxPhotoBytes = 2 * 1024 * 1024;
  String _maskedEmail = '';
  String? _error;
  String? _saveError;

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _firstNameController.dispose();
    _lastNameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    setState(() {
      _isLoading = true;
      _error = null;
    });

    final auth = context.read<AuthProvider>();

    try {
      final me = await _users.readMe();
      final twoFactor = await auth.readTwoFactorSetting();

      if (!mounted) return;

      setState(() {
        _me = me;
        _twoFactorEnabled = twoFactor.enabled;
        _maskedEmail = twoFactor.maskedEmail;
        _isLoading = false;
      });

      _resetControllers(me);
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _error = e.message;
        _isLoading = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _error = 'Could not reach the server. Check that the API is running.';
        _isLoading = false;
      });
    }
  }

  void _resetControllers(User me) {
    _firstNameController.text = me.firstName ?? '';
    _lastNameController.text = me.lastName ?? '';
    _emailController.text = me.email ?? '';
    _phoneController.text = me.phoneNumber ?? '';
  }

  Future<void> _save() async {
    setState(() => _saveError = null);

    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);

    try {
      final updated = await _users.updateMe(
        firstName: _firstNameController.text.trim(),
        lastName: _lastNameController.text.trim(),
        email: _emailController.text.trim(),
        phoneNumber: _phoneController.text.trim().isEmpty
            ? null
            : _phoneController.text.trim(),
      );

      if (!mounted) return;

      context.read<AuthProvider>().refreshDisplayName(
        updated.firstName,
        updated.lastName,
      );

      setState(() {
        _me = updated;
        _isEditing = false;
        _isSaving = false;
      });

      AppMessages.success(context, 'Your profile has been saved.');
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() {
        _saveError = e.message;
        _isSaving = false;
      });
    } catch (_) {
      if (!mounted) return;
      setState(() {
        _saveError =
            'Could not reach the server. Check that the API is running.';
        _isSaving = false;
      });
    }
  }

  Future<void> _toggleTwoFactor(bool enabled) async {
    setState(() => _isTogglingTwoFactor = true);

    try {
      final setting = await context.read<AuthProvider>().setTwoFactorSetting(
        enabled,
      );

      if (!mounted) return;

      setState(() {
        _twoFactorEnabled = setting.enabled;
        _maskedEmail = setting.maskedEmail;
        _isTogglingTwoFactor = false;
      });

      AppMessages.success(
        context,
        setting.enabled
            ? 'Two-factor sign-in is on. Next time we will email you a code.'
            : 'Two-factor sign-in is off.',
      );
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() => _isTogglingTwoFactor = false);
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _isTogglingTwoFactor = false);
      AppMessages.failure(
        context,
        'Could not change your security settings.',
      );
    }
  }

  Future<void> _changePhoto() async {
    XFile? picked;

    try {
      picked = await _picker.pickImage(
        source: ImageSource.gallery,
        maxWidth: 1024,
        maxHeight: 1024,
        imageQuality: 85,
      );
    } catch (_) {
      if (!mounted) return;
      AppMessages.failure(
        context,
        'Could not open your photos. Check the app has permission to see them.',
      );
      return;
    }

    if (picked == null || !mounted) {
      return;
    }

    final mimeType = _mimeTypeFor(picked.path);

    if (mimeType == null) {
      AppMessages.failure(context, 'Choose a JPEG or PNG image.');
      return;
    }

    final length = await File(picked.path).length();

    if (length > _maxPhotoBytes) {
      if (!mounted) return;
      AppMessages.failure(context, 'The image must be 2 MB or smaller.');
      return;
    }

    if (!mounted) return;

    setState(() => _isUploadingPhoto = true);

    try {
      await _users.uploadOwnPhoto(picked.path, mimeType);

      final resolved = ApiImage.resolve(_me?.profileImageUrl);

      if (resolved != null) {
        await NetworkImage(
          resolved,
          headers: ApiImage.authHeaders(),
        ).evict();
      }

      final refreshed = await _users.readMe();

      if (!mounted) return;

      setState(() {
        _me = refreshed;
        _photoVersion++;
        _isUploadingPhoto = false;
      });

      AppMessages.success(context, 'Your photo has been updated.');
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() => _isUploadingPhoto = false);
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _isUploadingPhoto = false);
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    }
  }

  static String? _mimeTypeFor(String path) {
    final lower = path.toLowerCase();

    if (lower.endsWith('.png')) return 'image/png';
    if (lower.endsWith('.jpg') || lower.endsWith('.jpeg')) return 'image/jpeg';

    return null;
  }

  Future<void> _deleteAccount() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Delete your account?'),
        content: const Text(
          'Your sign-in stops working straight away and you will be signed out. '
          'Past rides stay on record so receipts and reports still add up. '
          'This cannot be undone.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('KEEP MY ACCOUNT'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            style: FilledButton.styleFrom(backgroundColor: AppColors.danger),
            child: const Text('DELETE'),
          ),
        ],
      ),
    );

    if (confirmed != true || !mounted) {
      return;
    }

    setState(() => _isDeleting = true);

    final auth = context.read<AuthProvider>();
    final notifications = context.read<NotificationStore>();

    try {
      await _users.deleteMe();

      if (!mounted) return;

      AppMessages.success(context, 'Your account is closed. Signing you out.');

      await notifications.disconnect();
      await auth.logout();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() => _isDeleting = false);
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _isDeleting = false);
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    }
  }

  Future<void> _resign() async {
    final me = _me;

    if (me?.email == null) {
      return;
    }

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (dialogContext) => AlertDialog(
        title: const Text('Resign as a driver?'),
        content: const Text(
          'You will stop receiving ride requests and be taken off active duty. '
          'Your company has to take you back on.',
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(dialogContext).pop(false),
            child: const Text('CANCEL'),
          ),
          FilledButton(
            onPressed: () => Navigator.of(dialogContext).pop(true),
            child: const Text('RESIGN'),
          ),
        ],
      ),
    );

    if (confirmed != true || !mounted) {
      return;
    }

    setState(() => _isResigning = true);

    final auth = context.read<AuthProvider>();
    final notifications = context.read<NotificationStore>();

    try {
      final self = await _drivers.findByEmail(me!.email!);

      if (self?.id == null) {
        throw ApiClientException('Your driver record could not be found.');
      }

      await _drivers.resign(self!.id!);

      if (!mounted) return;

      AppMessages.success(context, 'You have resigned. Signing you out.');

      await notifications.disconnect();
      await auth.logout();
    } on ApiClientException catch (e) {
      if (!mounted) return;
      setState(() => _isResigning = false);
      AppMessages.failure(context, e.message);
    } catch (_) {
      if (!mounted) return;
      setState(() => _isResigning = false);
      AppMessages.failure(
        context,
        'Could not reach the server. Check that the API is running.',
      );
    }
  }

  Future<void> _changePassword() async {
    final changed = await ChangePasswordScreen.open(context);

    if (changed == true && mounted) {
      AppMessages.success(context, 'Your password has been changed.');
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return const Center(
        child: SizedBox(
          height: 28,
          width: 28,
          child: CircularProgressIndicator(strokeWidth: 2.5),
        ),
      );
    }

    final me = _me;

    if (_error != null || me == null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xl),
          child: Text(
            _error ?? 'Your profile could not be loaded.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Theme.of(context).colorScheme.error),
          ),
        ),
      );
    }

    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _load,
      child: ListView(
        padding: const EdgeInsets.all(AppSpacing.screenPadding),
        children: [
          Row(
            children: [
              _AvatarPicker(
                url: me.profileImageUrl,
                version: _photoVersion,
                isBusy: _isUploadingPhoto,
                onTap: _isUploadingPhoto ? null : _changePhoto,
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      me.fullName,
                      style: theme.textTheme.headlineSmall,
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    Text(
                      me.role ?? '',
                      style: theme.textTheme.bodySmall,
                    ),
                    Text(
                      '@${me.username ?? ''}',
                      style: theme.textTheme.bodySmall,
                    ),
                  ],
                ),
              ),
            ],
          ),
          const Divider(height: AppSpacing.xl),
          Row(
            children: [
              Expanded(
                child: Text('Your details', style: theme.textTheme.titleMedium),
              ),
              if (!_isEditing)
                TextButton.icon(
                  onPressed: () => setState(() => _isEditing = true),
                  icon: const Icon(Icons.edit_outlined, size: 16),
                  label: const Text('EDIT'),
                ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          if (_isEditing) _buildForm(theme) else _buildReadOnly(theme, me),
          const Divider(height: AppSpacing.xl),
          Text('Security', style: theme.textTheme.titleMedium),
          const SizedBox(height: AppSpacing.sm),
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            value: _twoFactorEnabled,
            onChanged: _isTogglingTwoFactor ? null : _toggleTwoFactor,
            title: const Text('Two-factor sign-in'),
            subtitle: Text(
              _twoFactorEnabled
                  ? 'You will be asked for a code emailed to $_maskedEmail.'
                  : 'Ask for an emailed code every time I sign in.',
            ),
          ),
          const SizedBox(height: AppSpacing.sm),
          OutlinedButton.icon(
            onPressed: _changePassword,
            icon: const Icon(Icons.password, size: 18),
            label: const Text('CHANGE PASSWORD'),
          ),
          if (me.role == Roles.driver) ...[
            const Divider(height: AppSpacing.xl),
            Text('Leaving', style: theme.textTheme.titleMedium),
            const SizedBox(height: AppSpacing.xs),
            Text(
              'Finish any ride and end your shift first — you cannot resign '
              'mid-ride.',
              style: theme.textTheme.bodySmall,
            ),
            const SizedBox(height: AppSpacing.sm),
            OutlinedButton.icon(
              onPressed: _isResigning ? null : _resign,
              style: OutlinedButton.styleFrom(
                foregroundColor: AppColors.danger,
                side: const BorderSide(color: AppColors.danger),
              ),
              icon: const Icon(Icons.logout, size: 18),
              label: Text(_isResigning ? 'RESIGNING…' : 'RESIGN'),
            ),
          ],
          const Divider(height: AppSpacing.xl),
          Text('Your account', style: theme.textTheme.titleMedium),
          const SizedBox(height: AppSpacing.xs),
          Text(
            'Deleting your account signs you out for good. Finish any ride that '
            'is under way first.',
            style: theme.textTheme.bodySmall,
          ),
          const SizedBox(height: AppSpacing.sm),
          OutlinedButton.icon(
            onPressed: _isDeleting ? null : _deleteAccount,
            style: OutlinedButton.styleFrom(
              foregroundColor: AppColors.danger,
              side: const BorderSide(color: AppColors.danger),
            ),
            icon: const Icon(Icons.delete_outline, size: 18),
            label: Text(_isDeleting ? 'DELETING…' : 'DELETE MY ACCOUNT'),
          ),
        ],
      ),
    );
  }

  Widget _buildReadOnly(ThemeData theme, User me) {
    return Column(
      children: [
        _Row(icon: Icons.badge_outlined, label: 'Name', value: me.fullName),
        _Row(
          icon: Icons.mail_outline,
          label: 'Email',
          value: me.email ?? '',
        ),
        _Row(
          icon: Icons.phone_outlined,
          label: 'Phone',
          value: (me.phoneNumber ?? '').isEmpty ? 'Not set' : me.phoneNumber!,
        ),
      ],
    );
  }

  Widget _buildForm(ThemeData theme) {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          TextFormField(
            controller: _firstNameController,
            enabled: !_isSaving,
            decoration: const InputDecoration(
              hintText: 'First name',
              prefixIcon: Icon(Icons.person_outline),
            ),
            validator: (value) => Validators.required(value, 'First name'),
          ),
          const SizedBox(height: AppSpacing.md),
          TextFormField(
            controller: _lastNameController,
            enabled: !_isSaving,
            decoration: const InputDecoration(
              hintText: 'Last name',
              prefixIcon: Icon(Icons.person_outline),
            ),
            validator: (value) => Validators.required(value, 'Last name'),
          ),
          const SizedBox(height: AppSpacing.md),
          TextFormField(
            controller: _emailController,
            enabled: !_isSaving,
            keyboardType: TextInputType.emailAddress,
            decoration: const InputDecoration(
              hintText: 'Email',
              prefixIcon: Icon(Icons.mail_outline),
            ),
            validator: Validators.email,
          ),
          const SizedBox(height: AppSpacing.md),
          TextFormField(
            controller: _phoneController,
            enabled: !_isSaving,
            keyboardType: TextInputType.phone,
            decoration: const InputDecoration(
              hintText: 'Phone number (optional)',
              prefixIcon: Icon(Icons.phone_outlined),
            ),
            validator: (value) => (value ?? '').trim().isEmpty
                ? null
                : Validators.phone(value),
          ),
          if (_saveError != null)
            Padding(
              padding: const EdgeInsets.only(top: AppSpacing.md),
              child: Text(
                _saveError!,
                style: TextStyle(
                  color: theme.colorScheme.error,
                  fontSize: 13,
                ),
              ),
            ),
          const SizedBox(height: AppSpacing.lg),
          FilledButton(
            onPressed: _isSaving ? null : _save,
            child: _isSaving
                ? const SizedBox(
                    height: 20,
                    width: 20,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : const Text('SAVE'),
          ),
          const SizedBox(height: AppSpacing.sm),
          OutlinedButton(
            onPressed: _isSaving
                ? null
                : () {
                    _resetControllers(_me!);
                    setState(() {
                      _isEditing = false;
                      _saveError = null;
                    });
                  },
            child: const Text('CANCEL'),
          ),
        ],
      ),
    );
  }
}

class _Row extends StatelessWidget {
  const _Row({required this.icon, required this.label, required this.value});

  final IconData icon;
  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 16, color: AppColors.textMuted),
          const SizedBox(width: AppSpacing.sm),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(label, style: theme.textTheme.bodySmall),
                Text(
                  value,
                  style: theme.textTheme.bodyMedium,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _AvatarPicker extends StatelessWidget {
  const _AvatarPicker({
    required this.url,
    required this.version,
    required this.isBusy,
    required this.onTap,
  });

  final String? url;
  final int version;
  final bool isBusy;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Semantics(
      button: true,
      label: 'Change your photo',
      child: InkWell(
        onTap: onTap,
        customBorder: const CircleBorder(),
        child: Stack(
          alignment: Alignment.bottomRight,
          children: [
            ApiImage.logo(
              key: ValueKey('avatar-$version'),
              url: url,
              size: 64,
              fallbackIcon: Icons.person,
            ),
            Container(
              padding: const EdgeInsets.all(4),
              decoration: BoxDecoration(
                color: AppColors.mobileHeader,
                shape: BoxShape.circle,
                border: Border.all(color: AppColors.surface, width: 2),
              ),
              child: isBusy
                  ? const SizedBox(
                      height: 12,
                      width: 12,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : const Icon(
                      Icons.photo_camera,
                      size: 12,
                      color: Colors.white,
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
