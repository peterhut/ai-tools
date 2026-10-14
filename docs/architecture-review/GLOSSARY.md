# Architecture Assessment

Evidence-based assessment of maintainability concerns to inform maintenance priorities.

## Language

**Architecture assessment**:
An evidence-backed investigation of selected maintainability concerns in their code and change-history context. Its outcome informs maintenance priorities rather than constituting a merge verdict.

**Change pressure**:
Demand for changes in an area of the codebase, evidenced by recent work or explicitly identified upcoming requirements. It includes planned work beyond historical change frequency; neither source establishes a maintainability problem by itself.

**Maintenance recommendation**:
An evidence-backed proposal for prioritizing a maintainability concern. It informs a decision about whether and when to act without constituting a detailed remediation design.

**Inferred ownership boundary**:
A division of responsibility supported by code evidence but not established by an explicit architectural rule or decision. A concern based on it is a reasoned risk rather than an explicit rule violation.

**Demonstrated degradation**:
Worsening maintainability supported by comparable earlier and current evidence. A concern visible only in the present code does not establish that worsening occurred.
