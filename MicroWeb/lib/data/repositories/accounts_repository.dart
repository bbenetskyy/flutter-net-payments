import '../models/responses/account_response.dart';

/// Abstraction for Accounts service REST calls routed via API Gateway base URL.
abstract class AccountsRepository {
  /// Returns current user's accounts (IBANs)
  Future<List<AccountResponse>> listMyAccounts({Map<String, dynamic>? query});

  /// Returns accounts (IBANs) for a specific user by their id
  Future<List<AccountResponse>> listUserAccounts(String userId, {Map<String, dynamic>? query});

  /// Optional helper to decode list to models
  static List<AccountResponse> parseList(dynamic data) => AccountResponse.listFromJson(data);
}
