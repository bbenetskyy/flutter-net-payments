import 'package:json_annotation/json_annotation.dart';
import 'package:uuid/uuid_value.dart';

import '../card_options.dart';
import '../card_type.dart';
import '../converters/uuid_value_converter.dart';

part 'card_response.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class CardResponse {
  /// Returns a new [CardResponse] instance.
  CardResponse({
    required this.id,
    required this.type,
    required this.name,
    required this.singleTransactionLimit,
    required this.monthlyLimit,
    required this.assignedUserId,
    required this.options,
    required this.printed,
    required this.terminated,
    required this.createdAt,
    this.updatedAt,
  });

  @JsonKey(name: r'id', required: true, includeIfNull: false)
  final String id;

  @JsonKey(name: r'type', required: true, includeIfNull: false)
  final CardType type;

  @JsonKey(name: r'name', required: true, includeIfNull: false)
  final String name;

  @JsonKey(name: r'singleTransactionLimit', required: true, includeIfNull: false)
  final int singleTransactionLimit;

  @JsonKey(name: r'monthlyLimit', required: true, includeIfNull: false)
  final int monthlyLimit;

  @JsonKey(name: r'assignedUserId', required: false, includeIfNull: false)
  final String? assignedUserId;

  // Flagged options: backend sends combined int. Parse into Set<CardOptions>.
  @JsonKey(name: r'options', required: true, includeIfNull: false, fromJson: optionsFromJson, toJson: optionsToJson)
  final Set<CardOptions>? options;

  @JsonKey(name: r'printed', required: true, includeIfNull: false)
  final bool printed;

  @JsonKey(name: r'terminated', required: true, includeIfNull: false)
  final bool terminated;

  @JsonKey(name: r'createdAt', required: true, includeIfNull: false)
  final DateTime createdAt;

  @JsonKey(name: r'updatedAt', required: false, includeIfNull: false)
  final DateTime? updatedAt;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is CardResponse &&
          other.id == id &&
          other.type == type &&
          other.name == name &&
          other.singleTransactionLimit == singleTransactionLimit &&
          other.monthlyLimit == monthlyLimit &&
          other.assignedUserId == assignedUserId &&
          other.options == options &&
          other.printed == printed &&
          other.terminated == terminated &&
          other.createdAt == createdAt &&
          other.updatedAt == updatedAt;

  @override
  int get hashCode =>
      id.hashCode +
      type.hashCode +
      name.hashCode +
      singleTransactionLimit.hashCode +
      monthlyLimit.hashCode +
      (assignedUserId == null ? 0 : assignedUserId.hashCode) +
      options.hashCode +
      printed.hashCode +
      terminated.hashCode +
      createdAt.hashCode +
      (updatedAt == null ? 0 : updatedAt.hashCode);

  factory CardResponse.fromJson(Map<String, dynamic> json) => _$CardResponseFromJson(json);

  Map<String, dynamic> toJson() => _$CardResponseToJson(this);

  static List<CardResponse> listFromJson(dynamic data) {
    if (data is List) {
      return data.map((e) => CardResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['items'] is List) {
      return (data['items'] as List).map((e) => CardResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map && data['data'] is List) {
      return (data['data'] as List).map((e) => CardResponse.fromJson(Map<String, dynamic>.from(e as Map))).toList();
    }
    if (data is Map<String, dynamic>) {
      return [CardResponse.fromJson(data)];
    }
    return const [];
  }

  @override
  String toString() {
    return toJson().toString();
  }
}
