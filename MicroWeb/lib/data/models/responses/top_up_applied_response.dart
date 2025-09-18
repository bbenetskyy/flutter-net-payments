import 'package:json_annotation/json_annotation.dart';
import 'package:micro_web/data/models/currency.dart';

part 'top_up_applied_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class TopUpAppliedResponse {
  TopUpAppliedResponse({this.status, this.correlationId, this.walletId, this.userId, this.currency, this.balanceMinor});

  @JsonKey(name: r'status', required: false, includeIfNull: false)
  final String? status;
  @JsonKey(name: r'correlationId', required: false, includeIfNull: false)
  final String? correlationId;
  @JsonKey(name: r'walletId', required: false, includeIfNull: false)
  final String? walletId;
  @JsonKey(name: r'userId', required: false, includeIfNull: false)
  final String? userId;
  @JsonKey(name: r'currency', required: false, includeIfNull: false)
  final Currency? currency;
  @JsonKey(name: r'balanceMinor', required: false, includeIfNull: false)
  final int? balanceMinor;

  factory TopUpAppliedResponse.fromJson(Map<String, dynamic> json) {
    final data = Map<String, dynamic>.from(json);
    final cur = data['currency'];
    if (cur is String) {
      final lc = cur.toUpperCase();
      switch (lc) {
        case 'EUR':
          data['currency'] = 0;
          break;
        case 'USD':
          data['currency'] = 1;
          break;
        case 'PLN':
          data['currency'] = 2;
          break;
        case 'GBP':
          data['currency'] = 3;
          break;
        default:
          final asInt = int.tryParse(cur);
          if (asInt != null) {
            data['currency'] = asInt;
          }
      }
    }
    return _$TopUpAppliedResponseFromJson(data);
  }
  Map<String, dynamic> toJson() => _$TopUpAppliedResponseToJson(this);
}
