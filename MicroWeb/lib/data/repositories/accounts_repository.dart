import '../models/responses/account_response.dart';

/// Abstraction for Accounts service REST calls routed via API Gateway base URL.
abstract class AccountsRepository {
  /// Returns current user's accounts (IBANs)
  Future<dynamic> listMyAccounts({Map<String, dynamic>? query});

  /// Optional helper to decode list to models
  static List<AccountResponse> parseList(dynamic data) => AccountResponse.listFromJson(data);
}
