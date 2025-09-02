//
// AUTO-GENERATED FILE, DO NOT MODIFY!
//

// ignore_for_file: unused_element
import 'package:json_annotation/json_annotation.dart';

part 'verification_decision_request.g.dart';


@JsonSerializable(
  checked: true,
  createToJson: true,
  disallowUnrecognizedKeys: false,
  explicitToJson: true,
)
class VerificationDecisionRequest {
  /// Returns a new [VerificationDecisionRequest] instance.
  VerificationDecisionRequest({

     this.code,

     this.accept,
  });

  @JsonKey(
    
    name: r'code',
    required: false,
    includeIfNull: false,
  )


  final String? code;



  @JsonKey(
    
    name: r'accept',
    required: false,
    includeIfNull: false,
  )


  final bool? accept;





    @override
    bool operator ==(Object other) => identical(this, other) || other is VerificationDecisionRequest &&
      other.code == code &&
      other.accept == accept;

    @override
    int get hashCode =>
        (code == null ? 0 : code.hashCode) +
        accept.hashCode;

  factory VerificationDecisionRequest.fromJson(Map<String, dynamic> json) => _$VerificationDecisionRequestFromJson(json);

  Map<String, dynamic> toJson() => _$VerificationDecisionRequestToJson(this);

  @override
  String toString() {
    return toJson().toString();
  }

}

