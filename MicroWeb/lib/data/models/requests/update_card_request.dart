//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

import '../card_options.dart';
import '../card_type.dart';

part 'update_card_request.g.dart';

@JsonSerializable(checked: true, createToJson: true, disallowUnrecognizedKeys: false, explicitToJson: true)
class UpdateCardRequest {
  /// Returns a new [UpdateCardRequest] instance.
  UpdateCardRequest({
    this.type,
    this.name,
    this.singleTransactionLimit,
    this.monthlyLimit,
    this.options,
    this.printed,
    this.assignedUserId,
    this.assignedUserIdSet,
  });

  @JsonKey(name: r'type', required: false, includeIfNull: false)
  final CardType? type;

  @JsonKey(name: r'name', required: false, includeIfNull: false)
  final String? name;

  @JsonKey(name: r'singleTransactionLimit', required: false, includeIfNull: false)
  final double? singleTransactionLimit;

  @JsonKey(name: r'monthlyLimit', required: false, includeIfNull: false)
  final double? monthlyLimit;

  // Flags: backend expects combined integer bitmask of CardOptions
  @JsonKey(name: r'options', required: false, includeIfNull: false, fromJson: optionsFromJson, toJson: optionsToJson)
  final Set<CardOptions>? options;

  @JsonKey(name: r'printed', required: false, includeIfNull: false)
  final bool? printed;

  @JsonKey(name: r'assignedUserId', required: false, includeIfNull: false)
  final String? assignedUserId;

  @JsonKey(name: r'assignedUserIdSet', required: false, includeIfNull: false)
  final bool? assignedUserIdSet;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is UpdateCardRequest &&
          other.type == type &&
          other.name == name &&
          other.singleTransactionLimit == singleTransactionLimit &&
          other.monthlyLimit == monthlyLimit &&
          other.options == options &&
          other.printed == printed &&
          other.assignedUserId == assignedUserId &&
          other.assignedUserIdSet == assignedUserIdSet;

  @override
  int get hashCode =>
      type.hashCode +
      (name == null ? 0 : name.hashCode) +
      (singleTransactionLimit == null ? 0 : singleTransactionLimit.hashCode) +
      (monthlyLimit == null ? 0 : monthlyLimit.hashCode) +
      options.hashCode +
      (printed == null ? 0 : printed.hashCode) +
      (assignedUserId == null ? 0 : assignedUserId.hashCode) +
      assignedUserIdSet.hashCode;

  factory UpdateCardRequest.fromJson(Map<String, dynamic> json) => _$UpdateCardRequestFromJson(json);

  Map<String, dynamic> toJson() => _$UpdateCardRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }
}
